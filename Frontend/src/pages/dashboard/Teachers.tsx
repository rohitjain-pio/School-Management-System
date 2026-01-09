import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Plus, Mail, Search, Filter, Edit, Trash2, Eye, UserIcon } from "lucide-react";
import { useTeachers } from "@/hooks/useTeachers";
import TeachersSkeleton from "@/skeletons/TeachersSkeleton";
import AddTeacherPopup from "@/popups/teachers/AddTeacherPopup";
import EditTeacherPopup from "@/popups/teachers/EditTeacherPopup";
import ViewTeacherPopup from "@/popups/teachers/ViewTeacherPopup";
import DeleteTeacherPopup from "@/popups/teachers/DeleteTeacherPopup";

const Teachers: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [filterSubject, setFilterSubject] = useState("all");

  const {
    data: teachers,
    isLoading,
    isError,
    error,
    addTeacher,
    editTeacher,
    deleteTeacher,
  } = useTeachers();

  const [selectedTeacher, setSelectedTeacher] = useState(null);
  const [modal, setModal] = useState<"add" | "edit" | "view" | "delete" | null>(
    null
  );
  const [isOpen, setIsOpen] = useState(false);

  const openModal = (
    type: "add" | "edit" | "view" | "delete",
    teacher?: any
  ) => {
    setModal(type);
    setSelectedTeacher(teacher || null);
    setIsOpen(true);
  };

  const closeModal = () => {
    setModal(null);
    setSelectedTeacher(null);
    setIsOpen(false);
  };

  const handleAdd = async (newTeacher: any) => {
    await addTeacher({ newTeacher });
    closeModal();
  };

  const handleEdit = async (updatedTeacher: any) => {
    if (!selectedTeacher) return;
    await editTeacher({
      updatedTeacher: { ...selectedTeacher, ...updatedTeacher },
    });
    closeModal();
  };

  const handleDelete = async (id: string) => {
    await deleteTeacher({ id });
    closeModal();
  };

  const subjects = [
    "all",
    "Mathematics",
    "Physics",
    "English Literature",
    "Chemistry",
    "Biology",
    "History",
  ];

  const filteredTeachers = teachers?.filter((teacher) => {
    const matchesSearch =
      teacher.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      teacher.email.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesSubject =
      filterSubject === "all" || teacher.subject === filterSubject;
    return matchesSearch && matchesSubject;
  });
  const getGenderLabel = (gender: number) => {
    switch (gender) {
      case 0:
        return "Male";
      case 1:
        return "Female";
      case 2:
        return "Other";
      default:
        return "N/A";
    }
  };
  if (isLoading) return <TeachersSkeleton />;
  if (isError) return <h1>Error: {error.message}</h1>;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-bold text-gray-900">Teachers</h2>
          <p className="text-gray-600 mt-2">Manage your teaching staff</p>
        </div>
        <Button
          className="flex items-center space-x-2"
          onClick={() => openModal("add")}
        >
          <Plus className="h-4 w-4" />
          <span>Add New Teacher</span>
        </Button>
      </div>
      {Array.isArray(teachers) && teachers.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Teacher Directory</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col sm:flex-row gap-4 mb-6">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <input
                  type="text"
                  placeholder="Search teachers..."
                  className="pl-10 pr-4 py-2 border border-gray-300 rounded-lg w-full"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
              <div className="flex items-center gap-2">
                <Filter className="h-4 w-4 text-gray-400" />
                <select
                  className="px-3 py-2 border border-gray-300 rounded-lg"
                  value={filterSubject}
                  onChange={(e) => setFilterSubject(e.target.value)}
                >
                  {subjects.map((subject) => (
                    <option key={subject} value={subject}>
                      {subject === "all" ? "All Subjects" : subject}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Sr.No.</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Phone</TableHead>
                  <TableHead>Gender</TableHead>
                  <TableHead>Address</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredTeachers.map((teacher, index) => (
                  <TableRow key={teacher.id}>
                    <TableCell>{index + 1}</TableCell>
                    <TableCell>
                      <div>
                        <div className="font-medium">{teacher.name}</div>
                        <div className="text-sm text-gray-500 flex items-center">
                          <Mail className="h-3 w-3 mr-1" />
                          {teacher.email}
                        </div>
                      </div>
                    </TableCell>

                    <TableCell>{teacher.phone}</TableCell>
                    <TableCell>
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium">
                        {getGenderLabel(teacher.gender)}
                      </span>
                    </TableCell>
                    <TableCell>
                      <div className="text-sm text-gray-600 truncate max-w-xs">
                        {teacher.address}
                      </div>
                    </TableCell>

                    <TableCell>
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                        Present
                      </span>
                    </TableCell>
                    <TableCell>
                      <div className="flex space-x-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => openModal("view", teacher)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => openModal("edit", teacher)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => openModal("delete", teacher)}
                        >
                          <Trash2 className="h-4 w-4 text-red-500" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

      ) : (
        <div className="flex flex-col items-center justify-center text-center py-16 px-6 mt-12">
          <div className="bg-gray-200 rounded-full p-4 mb-4">
            <UserIcon className="h-12 w-12 text-primary-700" />
          </div>
          <h2 className="text-2xl sm:text-3xl font-bold text-primary-800 mb-2">
            No Teachers Found
          </h2>
          <p className="text-gray-600 text-base sm:text-lg max-w-md mb-6">
            🧑‍🏫 You haven’t added any teachers yet. Add a teacher to assign them to classes.
          </p>
          <Button
            onClick={() => openModal("add")}
            className="flex items-center space-x-2"
          >
            <Plus className="h-4 w-4" />
            <span>Add New Teacher</span>
          </Button>
        </div>

      )}

      {modal === "add" && (
        <AddTeacherPopup
          isOpen={isOpen}
          onClose={closeModal}
          onSubmit={handleAdd}
        />
      )}
      {modal === "edit" && selectedTeacher && (
        <EditTeacherPopup
          isOpen={isOpen}
          teacherData={selectedTeacher}
          onClose={closeModal}
          onSubmit={handleEdit}
        />
      )}
      {modal === "view" && selectedTeacher && (
        <ViewTeacherPopup
          isOpen={isOpen}
          teacherData={selectedTeacher}
          onClose={closeModal}
        />
      )}
      {modal === "delete" && selectedTeacher && (
        <DeleteTeacherPopup
          isOpen={isOpen}
          teacherData={selectedTeacher}
          onClose={closeModal}
          onConfirm={() => handleDelete(selectedTeacher.id)}
        />
      )}
    </div>
  );
};

export default Teachers;
