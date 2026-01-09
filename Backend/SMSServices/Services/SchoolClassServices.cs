using AutoMapper;
using SMSDataModel.Model.Models;
using SMSDataModel.Model.RequestDtos;
using SMSRepository.RepositoryInterfaces;
using SMSServices.ServicesInterfaces;

namespace SMSServices.Services
{
    public class SchoolClassServices : ISchoolClassServices
    {

        private readonly IClassRepository classRepository;
        private readonly IMapper mapper;
        public SchoolClassServices(IClassRepository classRepository, IMapper mapper)
        {
            this.classRepository = classRepository;
            this.mapper = mapper;
        }
        public async Task<List<SchoolClass>> GetAllClassesAsync(Guid schoolId)
        {
            var result = await classRepository.GetAllClassesAsync(schoolId);
            return result;
        }

        // Not now
        public async Task<SchoolClass> GetClassByIdAsync(Guid id)
        {
            var result = await classRepository.GetClassByIdAsync(id);
            if(result != null)
            {
                return result;
            }
            throw new Exception("Class with this Id not found");
        }
        public async Task<SchoolClass> CreateClassAsync(CreateClassRequestDto createClassRequestDto)
        {
            var newClass = mapper.Map<SchoolClass>(createClassRequestDto);
            var result = await classRepository.CreateClassAsync(newClass);
            return result;
        }
        public async Task<SchoolClass> UpdateClassAsync(Guid classId, UpdateClassRequestDto updatedClass)
        {
            var schoolClass = await classRepository.GetClassByIdAsync(classId);
            if (schoolClass != null)
            {
                schoolClass.Name = updatedClass.Name;
                schoolClass.Section = updatedClass.Section;
                schoolClass.ClassTeacherId = updatedClass.ClassTeacherId;
                var result = await classRepository.UpdateClassAsync(schoolClass);
                return result;
            }
            throw new Exception("Class with this Id not found");
        }
        public async Task<SchoolClass> DeleteClassAsync(Guid id)
        {

            var schoolClass = await classRepository.GetClassByIdAsync(id);
            if (schoolClass != null)
            {
                var result = await classRepository.DeleteClassAsync(schoolClass);
                return result;
            }
            throw new Exception("Class with this Id not found");
        }

    }
}
