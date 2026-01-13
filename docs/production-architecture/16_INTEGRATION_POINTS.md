# Integration Points
## Third-Party Services & APIs

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 10 minutes  
**Status:** 🟡 Partially Implemented

---

## 🎯 **Integration Overview**

Our system integrates with external services for payments, messaging, email, and cloud storage.

**Design Principles:**
1. **Abstraction** - Use interfaces for easy provider swaps
2. **Fallbacks** - Graceful degradation when services fail
3. **Security** - Never expose API keys in frontend
4. **Monitoring** - Track all external API calls
5. **Cost Control** - Rate limiting and usage alerts

---

## 💳 **Payment Gateway: Razorpay**

**Use Case:** School fee payments, subscriptions

### **Setup**

```bash
# Install Razorpay SDK
dotnet add package Razorpay
```

**Configuration:**

```csharp
// appsettings.json
{
  "Razorpay": {
    "KeyId": "rzp_live_xxxxxxxxxxxx",
    "KeySecret": "your_secret_key_here",
    "WebhookSecret": "webhook_secret_here"
  }
}

// Register service
builder.Services.AddSingleton<IPaymentService, RazorpayPaymentService>();
```

### **Implementation**

**Create Payment Order:**

```csharp
public class RazorpayPaymentService : IPaymentService
{
    private readonly RazorpayClient _client;
    private readonly ILogger<RazorpayPaymentService> _logger;
    
    public RazorpayPaymentService(IConfiguration config, ILogger<RazorpayPaymentService> logger)
    {
        var keyId = config["Razorpay:KeyId"];
        var keySecret = config["Razorpay:KeySecret"];
        _client = new RazorpayClient(keyId, keySecret);
        _logger = logger;
    }
    
    public async Task<PaymentOrder> CreateOrderAsync(CreatePaymentOrderRequest request)
    {
        try
        {
            var options = new Dictionary<string, object>
            {
                { "amount", request.Amount * 100 },  // Convert to paise (₹100 = 10000 paise)
                { "currency", "INR" },
                { "receipt", $"rcpt_{request.StudentId}_{DateTime.UtcNow.Ticks}" },
                { "payment_capture", 1 },  // Auto-capture
                { "notes", new Dictionary<string, string>
                    {
                        { "student_id", request.StudentId.ToString() },
                        { "school_id", request.SchoolId.ToString() },
                        { "fee_type", request.FeeType }
                    }
                }
            };
            
            var order = _client.Order.Create(options);
            
            _logger.LogInformation(
                "Payment order created: {OrderId} for student {StudentId}, amount ₹{Amount}",
                order["id"], request.StudentId, request.Amount);
            
            return new PaymentOrder
            {
                OrderId = order["id"].ToString(),
                Amount = request.Amount,
                Currency = "INR",
                Status = "created"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Razorpay order for student {StudentId}", request.StudentId);
            throw new PaymentException("Payment gateway unavailable. Please try again later.", ex);
        }
    }
    
    public async Task<bool> VerifyPaymentSignatureAsync(string orderId, string paymentId, string signature)
    {
        try
        {
            var secret = _configuration["Razorpay:KeySecret"];
            var payload = $"{orderId}|{paymentId}";
            
            var hash = ComputeHMACSHA256(payload, secret);
            
            return hash == signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment signature verification failed");
            return false;
        }
    }
    
    private string ComputeHMACSHA256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
```

**Frontend Integration:**

```typescript
// services/paymentService.ts
export async function initiatePayment(feeDetails: FeePaymentRequest) {
  // 1. Create order on backend
  const response = await api.post<PaymentOrder>('/api/payments/create-order', feeDetails);
  const order = response.data;
  
  // 2. Load Razorpay script
  const razorpay = await loadRazorpayScript();
  
  // 3. Open Razorpay payment modal
  const options = {
    key: import.meta.env.VITE_RAZORPAY_KEY_ID,
    amount: order.amount * 100,  // Paise
    currency: 'INR',
    name: 'ABC School',
    description: `${feeDetails.feeType} - ${feeDetails.studentName}`,
    order_id: order.orderId,
    handler: async function (response: RazorpayResponse) {
      // 4. Verify payment on backend
      await api.post('/api/payments/verify', {
        orderId: response.razorpay_order_id,
        paymentId: response.razorpay_payment_id,
        signature: response.razorpay_signature,
      });
      
      toast.success('Payment successful!');
    },
    prefill: {
      name: feeDetails.parentName,
      email: feeDetails.parentEmail,
      contact: feeDetails.parentPhone,
    },
    theme: {
      color: '#3399cc',
    },
  };
  
  const rzp = new razorpay(options);
  rzp.open();
}

function loadRazorpayScript(): Promise<any> {
  return new Promise((resolve) => {
    const script = document.createElement('script');
    script.src = 'https://checkout.razorpay.com/v1/checkout.js';
    script.onload = () => resolve(window.Razorpay);
    document.body.appendChild(script);
  });
}
```

**Webhook Handler:**

```csharp
[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> RazorpayWebhook([FromBody] RazorpayWebhookPayload payload)
{
    // Verify webhook signature
    var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
    var isValid = _paymentService.VerifyWebhookSignature(payload, signature);
    
    if (!isValid)
    {
        _logger.LogWarning("Invalid Razorpay webhook signature");
        return Unauthorized();
    }
    
    // Handle different events
    switch (payload.Event)
    {
        case "payment.captured":
            await HandlePaymentCapturedAsync(payload);
            break;
        
        case "payment.failed":
            await HandlePaymentFailedAsync(payload);
            break;
        
        case "refund.processed":
            await HandleRefundProcessedAsync(payload);
            break;
    }
    
    return Ok();
}
```

---

## 📱 **SMS Service: Twilio**

**Use Case:** Attendance alerts, fee reminders, emergency notifications

### **Setup**

```bash
dotnet add package Twilio
```

**Configuration:**

```csharp
// appsettings.json
{
  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your_auth_token",
    "PhoneNumber": "+919876543210"
  }
}
```

### **Implementation**

```csharp
public class TwilioSmsService : ISmsService
{
    private readonly TwilioRestClient _client;
    private readonly string _fromNumber;
    private readonly ILogger<TwilioSmsService> _logger;
    
    public TwilioSmsService(IConfiguration config, ILogger<TwilioSmsService> logger)
    {
        var accountSid = config["Twilio:AccountSid"];
        var authToken = config["Twilio:AuthToken"];
        _fromNumber = config["Twilio:PhoneNumber"];
        
        _client = new TwilioRestClient(accountSid, authToken);
        _logger = logger;
    }
    
    public async Task<bool> SendSmsAsync(string toNumber, string message)
    {
        try
        {
            // Validate Indian phone number format
            if (!toNumber.StartsWith("+91"))
                toNumber = "+91" + toNumber.TrimStart('0');
            
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(toNumber),
                client: _client
            );
            
            _logger.LogInformation(
                "SMS sent: {MessageSid} to {PhoneNumber}",
                messageResource.Sid, toNumber);
            
            return messageResource.Status != MessageResource.StatusEnum.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", toNumber);
            return false;
        }
    }
    
    public async Task SendBulkSmsAsync(List<string> phoneNumbers, string message)
    {
        // Send in batches of 50 to avoid rate limits
        var batches = phoneNumbers.Chunk(50);
        
        foreach (var batch in batches)
        {
            var tasks = batch.Select(phone => SendSmsAsync(phone, message));
            await Task.WhenAll(tasks);
            
            // Rate limiting: 50 SMS per second for Twilio
            await Task.Delay(1000);
        }
    }
}

// Usage: Send attendance alert
public async Task SendAbsentNotificationAsync(Guid studentId, DateTime date)
{
    var student = await _studentService.GetStudentByIdAsync(studentId);
    var parent = await _parentService.GetParentByStudentIdAsync(studentId);
    
    var message = $"Dear {parent.Name}, your child {student.FirstName} was marked absent on {date:dd-MMM-yyyy}. " +
                  $"Please contact school if this is incorrect. - {student.SchoolName}";
    
    await _smsService.SendSmsAsync(parent.PhoneNumber, message);
}
```

**Template Messages:**

```csharp
public static class SmsTemplates
{
    public static string AbsentNotification(string parentName, string studentName, DateTime date, string schoolName)
        => $"Dear {parentName}, {studentName} was absent on {date:dd-MMM-yyyy}. Contact school if incorrect. -{schoolName}";
    
    public static string FeeReminder(string parentName, decimal amount, DateTime dueDate, string schoolName)
        => $"Dear {parentName}, fee payment of ₹{amount} is due on {dueDate:dd-MMM-yyyy}. Pay at {schoolName} or online.";
    
    public static string ExamSchedule(string parentName, string examName, DateTime examDate, string schoolName)
        => $"Dear {parentName}, {examName} scheduled on {examDate:dd-MMM-yyyy}. Best wishes! -{schoolName}";
    
    public static string Emergency(string message, string schoolName)
        => $"URGENT: {message} -{schoolName}. Please respond immediately.";
}
```

---

## 📧 **Email Service: SendGrid**

**Use Case:** Reports, newsletters, password reset

### **Setup**

```bash
dotnet add package SendGrid
```

**Implementation:**

```csharp
public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;
    
    public SendGridEmailService(IConfiguration config)
    {
        var apiKey = config["SendGrid:ApiKey"];
        _client = new SendGridClient(apiKey);
        _fromEmail = config["SendGrid:FromEmail"];
        _fromName = config["SendGrid:FromName"];
    }
    
    public async Task<bool> SendEmailAsync(EmailMessage message)
    {
        try
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_fromEmail, _fromName),
                Subject = message.Subject,
                HtmlContent = message.HtmlBody
            };
            
            msg.AddTo(new EmailAddress(message.ToEmail, message.ToName));
            
            // Add attachments
            foreach (var attachment in message.Attachments)
            {
                msg.AddAttachment(
                    attachment.FileName,
                    Convert.ToBase64String(attachment.Content),
                    attachment.ContentType
                );
            }
            
            var response = await _client.SendEmailAsync(msg);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email sent to {Email}", message.ToEmail);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("SendGrid error: {StatusCode} - {Body}", response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", message.ToEmail);
            return false;
        }
    }
    
    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var resetLink = $"https://schoolms.com/reset-password?token={resetToken}";
        
        var htmlBody = $@"
            <h2>Password Reset Request</h2>
            <p>You requested to reset your password. Click the link below to proceed:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>This link expires in 1 hour.</p>
            <p>If you didn't request this, please ignore this email.</p>
        ";
        
        await SendEmailAsync(new EmailMessage
        {
            ToEmail = email,
            Subject = "Password Reset - School Management System",
            HtmlBody = htmlBody
        });
    }
    
    public async Task SendMonthlyReportAsync(Guid parentId)
    {
        var parent = await _parentService.GetParentByIdAsync(parentId);
        var students = await _studentService.GetStudentsByParentIdAsync(parentId);
        
        foreach (var student in students)
        {
            var report = await _reportService.GenerateMonthlyReportAsync(student.Id);
            var pdfBytes = await _pdfService.GeneratePdfAsync(report);
            
            await SendEmailAsync(new EmailMessage
            {
                ToEmail = parent.Email,
                ToName = parent.Name,
                Subject = $"Monthly Report - {student.FirstName} {student.LastName}",
                HtmlBody = $"<p>Dear {parent.Name},</p><p>Please find attached the monthly report for {student.FirstName}.</p>",
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        FileName = $"report_{student.FirstName}_{DateTime.Now:yyyyMM}.pdf",
                        Content = pdfBytes,
                        ContentType = "application/pdf"
                    }
                }
            });
        }
    }
}
```

---

## ☁️ **Cloud Storage: Azure Blob / AWS S3**

**Use Case:** Student photos, assignment uploads, report PDFs

### **Azure Blob Storage**

```csharp
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    
    public AzureBlobStorageService(IConfiguration config)
    {
        var connectionString = config["Azure:BlobStorage:ConnectionString"];
        _containerName = config["Azure:BlobStorage:ContainerName"];
        _blobServiceClient = new BlobServiceClient(connectionString);
    }
    
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, Guid schoolId)
    {
        try
        {
            // School-isolated path: school-{schoolId}/files/{fileName}
            var blobName = $"school-{schoolId}/files/{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}_{fileName}";
            
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            
            var blobClient = containerClient.GetBlobClient(blobName);
            
            // Upload with metadata
            var blobUploadOptions = new BlobUploadOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "school_id", schoolId.ToString() },
                    { "uploaded_at", DateTime.UtcNow.ToString("o") },
                    { "original_filename", fileName }
                }
            };
            
            await blobClient.UploadAsync(fileStream, blobUploadOptions);
            
            _logger.LogInformation("File uploaded: {BlobName} for school {SchoolId}", blobName, schoolId);
            
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            throw new FileStorageException("Failed to upload file", ex);
        }
    }
    
    public async Task<Stream> DownloadFileAsync(string blobUrl, Guid requestingSchoolId)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            
            // Verify school ownership
            var properties = await blobClient.GetPropertiesAsync();
            var fileSchoolId = Guid.Parse(properties.Value.Metadata["school_id"]);
            
            if (fileSchoolId != requestingSchoolId)
            {
                throw new UnauthorizedAccessException("Cannot access file from different school");
            }
            
            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {BlobUrl}", blobUrl);
            throw;
        }
    }
    
    public async Task DeleteFileAsync(string blobUrl, Guid requestingSchoolId)
    {
        var blobClient = new BlobClient(new Uri(blobUrl));
        
        // Verify ownership before deletion
        var properties = await blobClient.GetPropertiesAsync();
        var fileSchoolId = Guid.Parse(properties.Value.Metadata["school_id"]);
        
        if (fileSchoolId != requestingSchoolId)
        {
            throw new UnauthorizedAccessException("Cannot delete file from different school");
        }
        
        await blobClient.DeleteIfExistsAsync();
    }
}
```

### **File Upload Controller**

```csharp
[HttpPost("upload")]
[Authorize]
[RequestSizeLimit(5_000_000)]  // 5MB max
public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
{
    try
    {
        var schoolId = GetUserSchoolId();
        
        // Validate file
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");
        
        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest("File type not allowed");
        
        // Upload to cloud
        using var stream = file.OpenReadStream();
        var fileUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, schoolId);
        
        // Save metadata to database
        var fileRecord = new FileMetadata
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FileUrl = fileUrl,
            FileSize = file.Length,
            ContentType = file.ContentType,
            SchoolId = schoolId,
            UploadedBy = GetUserId(),
            UploadedAt = DateTime.UtcNow
        };
        
        await _fileRepository.AddAsync(fileRecord);
        
        return Ok(new { fileUrl, fileId = fileRecord.Id });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "File upload failed");
        return StatusCode(500, "File upload failed");
    }
}
```

---

## 🔔 **Push Notifications: Firebase Cloud Messaging**

**Use Case:** Real-time mobile app notifications

```csharp
public class FirebaseNotificationService : IPushNotificationService
{
    private readonly FirebaseMessaging _messaging;
    
    public async Task SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string> data)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data,
            Android = new AndroidConfig
            {
                Priority = Priority.High
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Badge = 1,
                    Sound = "default"
                }
            }
        };
        
        var response = await _messaging.SendAsync(message);
        _logger.LogInformation("FCM notification sent: {Response}", response);
    }
    
    // Send to all parents when school makes announcement
    public async Task SendBulkNotificationAsync(Guid schoolId, string title, string message)
    {
        var deviceTokens = await _deviceTokenRepository.GetTokensBySchoolIdAsync(schoolId);
        
        var multicastMessage = new MulticastMessage
        {
            Tokens = deviceTokens,
            Notification = new Notification
            {
                Title = title,
                Body = message
            }
        };
        
        var response = await _messaging.SendMulticastAsync(multicastMessage);
        _logger.LogInformation("Sent {SuccessCount}/{TotalCount} notifications", 
            response.SuccessCount, deviceTokens.Count);
    }
}
```

---

## 📊 **Integration Monitoring**

**Track external API health:**

```csharp
public class IntegrationHealthCheck : IHealthCheck
{
    private readonly IPaymentService _paymentService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, string>();
        var isHealthy = true;
        
        // Check Razorpay
        try
        {
            await _paymentService.GetPaymentStatusAsync("test");
            results["Razorpay"] = "Healthy";
        }
        catch
        {
            results["Razorpay"] = "Unhealthy";
            isHealthy = false;
        }
        
        // Check Twilio
        try
        {
            await _smsService.CheckBalanceAsync();
            results["Twilio"] = "Healthy";
        }
        catch
        {
            results["Twilio"] = "Unhealthy";
            isHealthy = false;
        }
        
        // Check SendGrid
        try
        {
            await _emailService.ValidateApiKeyAsync();
            results["SendGrid"] = "Healthy";
        }
        catch
        {
            results["SendGrid"] = "Unhealthy";
            isHealthy = false;
        }
        
        return isHealthy
            ? HealthCheckResult.Healthy("All integrations healthy", results)
            : HealthCheckResult.Degraded("Some integrations unhealthy", data: results);
    }
}

// Register
builder.Services.AddHealthChecks()
    .AddCheck<IntegrationHealthCheck>("integrations");

// Endpoint
app.MapHealthChecks("/health/integrations");
```

---

## 💰 **Cost Monitoring**

**Track spending on external services:**

```sql
-- Monthly integration costs
SELECT 
    'Razorpay' as Service,
    COUNT(*) as Transactions,
    SUM(Amount) as TotalAmount,
    SUM(Amount * 0.02) as EstimatedFees  -- 2% fee
FROM Payments
WHERE CreatedAt >= DATEADD(MONTH, -1, GETDATE())

UNION ALL

SELECT 
    'Twilio' as Service,
    COUNT(*) as SmsCount,
    NULL as TotalAmount,
    COUNT(*) * 0.50 as EstimatedCost  -- ₹0.50 per SMS
FROM SmsLogs
WHERE SentAt >= DATEADD(MONTH, -1, GETDATE())

UNION ALL

SELECT 
    'SendGrid' as Service,
    COUNT(*) as EmailCount,
    NULL,
    COUNT(*) * 0.01 as EstimatedCost  -- ₹0.01 per email
FROM EmailLogs
WHERE SentAt >= DATEADD(MONTH, -1, GETDATE());
```

---

## ✅ **Integration Checklist**

```
Setup:
[ ] Razorpay account created, API keys configured
[ ] Twilio account created, phone number purchased
[ ] SendGrid account created, sender verified
[ ] Azure Blob Storage container created
[ ] Firebase project created for push notifications

Security:
[ ] API keys stored in Azure Key Vault (not appsettings.json)
[ ] Webhook endpoints use signature verification
[ ] File uploads validate size and type
[ ] Cloud storage uses school-specific folders

Monitoring:
[ ] Health checks configured for all integrations
[ ] Cost tracking dashboard set up
[ ] Alerts for integration failures
[ ] Usage quotas monitored

Testing:
[ ] Payment flow tested end-to-end
[ ] SMS delivery verified
[ ] Email delivery tested (check spam folder)
[ ] File upload/download tested
```

---

**Document Status:** ✅ Complete  
**Integrations Active:** 🟡 Partial (Payment ✅, SMS ❌, Email ❌)  
**Priority:** Complete before production launch