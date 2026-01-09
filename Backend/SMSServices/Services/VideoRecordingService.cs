using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace SMSServices.Services
{
    public interface IVideoRecordingService
    {
        Task<string> StartRecording(Guid roomId, Guid userId, string username);
        Task StopRecording(string recordingId);
        Task<RecordingInfo?> GetRecordingInfo(string recordingId);
        Task<List<RecordingInfo>> GetRoomRecordings(Guid roomId);
        bool IsRecording(Guid roomId);
    }

    public class RecordingInfo
    {
        public string RecordingId { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public Guid StartedBy { get; set; }
        public string StartedByUsername { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string Status { get; set; } = "Recording"; // Recording, Completed, Failed
    }

    public class VideoRecordingService : IVideoRecordingService
    {
        private readonly IConfiguration _configuration;
        private readonly string _recordingsPath;
        private readonly ConcurrentDictionary<Guid, RecordingInfo> _activeRecordings;
        private readonly ConcurrentDictionary<string, RecordingInfo> _allRecordings;

        public VideoRecordingService(IConfiguration configuration)
        {
            _configuration = configuration;
            _recordingsPath = _configuration["VideoRecordings:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Recordings");
            _activeRecordings = new ConcurrentDictionary<Guid, RecordingInfo>();
            _allRecordings = new ConcurrentDictionary<string, RecordingInfo>();

            // Ensure recordings directory exists
            Directory.CreateDirectory(_recordingsPath);
        }

        public async Task<string> StartRecording(Guid roomId, Guid userId, string username)
        {
            // Check if already recording
            if (_activeRecordings.ContainsKey(roomId))
            {
                throw new InvalidOperationException("Recording already in progress for this room");
            }

            var recordingId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Room_{roomId}_{timestamp}.webm";
            var filePath = Path.Combine(_recordingsPath, fileName);

            var recordingInfo = new RecordingInfo
            {
                RecordingId = recordingId,
                RoomId = roomId,
                StartedBy = userId,
                StartedByUsername = username,
                StartTime = DateTime.UtcNow,
                FilePath = filePath,
                Status = "Recording"
            };

            _activeRecordings.TryAdd(roomId, recordingInfo);
            _allRecordings.TryAdd(recordingId, recordingInfo);

            // In a real implementation, you would:
            // 1. Use FFmpeg or similar to capture video streams
            // 2. Store to cloud storage (Azure Blob, AWS S3)
            // 3. Generate thumbnails
            // 4. Create metadata files

            // For now, just create a placeholder file
            await File.WriteAllTextAsync(filePath, $"Recording started at {recordingInfo.StartTime}");

            return recordingId;
        }

        public async Task StopRecording(string recordingId)
        {
            if (!_allRecordings.TryGetValue(recordingId, out var recordingInfo))
            {
                throw new InvalidOperationException("Recording not found");
            }

            recordingInfo.EndTime = DateTime.UtcNow;
            recordingInfo.Status = "Completed";

            // Calculate file size
            if (File.Exists(recordingInfo.FilePath))
            {
                var fileInfo = new FileInfo(recordingInfo.FilePath);
                recordingInfo.FileSizeBytes = fileInfo.Length;
            }

            _activeRecordings.TryRemove(recordingInfo.RoomId, out _);

            // In a real implementation:
            // 1. Finalize the video file
            // 2. Upload to cloud storage
            // 3. Generate thumbnails
            // 4. Update database with metadata
            // 5. Notify participants recording is available

            await Task.CompletedTask;
        }

        public async Task<RecordingInfo?> GetRecordingInfo(string recordingId)
        {
            _allRecordings.TryGetValue(recordingId, out var info);
            return await Task.FromResult(info);
        }

        public async Task<List<RecordingInfo>> GetRoomRecordings(Guid roomId)
        {
            var recordings = _allRecordings.Values
                .Where(r => r.RoomId == roomId)
                .OrderByDescending(r => r.StartTime)
                .ToList();

            return await Task.FromResult(recordings);
        }

        public bool IsRecording(Guid roomId)
        {
            return _activeRecordings.ContainsKey(roomId);
        }
    }
}
