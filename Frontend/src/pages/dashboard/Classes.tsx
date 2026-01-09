import React, { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Plus,
  Book,
  Eye,
  Pencil,
  Trash2,
  GraduationCap,
  CalendarCheck2,
  CalendarCheck,
} from "lucide-react";
import { useClasses } from "@/hooks/useClasses";
import ClassesSkeleton from "@/skeletons/ClassesSkeleton";
import AddClassPopup from "@/popups/classes/AddClassPopup";
import EditClassPopup from "@/popups/classes/EditClassPopup";
import ViewClassPopup from "@/popups/classes/ViewClassPopup";
import DeleteClassPopup from "@/popups/classes/DeleteClassPopup";
import { Link } from "react-router-dom";

import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { useStudents } from "@/hooks/useStudents";
import EmptyState from "@/components/EmptyState";
const server_url = import.meta.env.VITE_API_URL;

interface ClassesProps {
  schoolId: string;
}

interface ClassItem {
  id: string;
  name: string;
  section: string;
  classTeacherId: string;
  schoolId: string;
}

const Classes: React.FC = () => {
  const {
    data: classes,
    isLoading,
    error,
    addClass,
    editClass,
    deleteClass,
  } = useClasses();

  const [selectedClass, setSelectedClass] = useState<ClassItem | null>(null);
  const [modal, setModal] = useState<"add" | "edit" | "view" | "delete" | null>(
    null
  );
  const [isOpen, setIsOpen] = useState(false);

  const { data: students } = useStudents();

  const openModal = (
    type: "add" | "edit" | "view" | "delete",
    classItem?: ClassItem
  ) => {
    setIsOpen(true);
    setSelectedClass(classItem || null);
    setModal(type);
  };

  const closeModal = () => {
    setIsOpen(false);
    setSelectedClass(null);
    setModal(null);
  };

  const handleAdd = async (newClass: ClassItem) => {
    try {
      console.log("Creating:", newClass);
      const result = await addClass({ newClass });
      console.log("Created class:", result);
      closeModal();
    } catch (err) {
      console.error(err instanceof Error ? err.message : "Failed to add class");
    }
  };

  const handleEdit = async (updatedClass: ClassItem) => {
    if (!selectedClass) return;
    try {
      const classWithId = { ...selectedClass, ...updatedClass };
      console.log("Sending updated class:", classWithId);
      await editClass({ updatedClass: classWithId });
      closeModal();
    } catch (err) {
      console.error(
        err instanceof Error ? err.message : "Failed to update class"
      );
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteClass({ id });
      closeModal();
    } catch (err) {
      console.error(
        err instanceof Error ? err.message : "Failed to delete class"
      );
    }
  };

  if (isLoading) return <ClassesSkeleton />;

  if (error) return <div>Error loading classes: {error.message}</div>;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl sm:text-3xl font-bold text-gray-900">
            Classes
          </h2>
          <p className="text-gray-600 mt-2">
            Manage all your classes and schedules
          </p>
        </div>
        <Button
          onClick={() => openModal("add")}
          className="flex items-center space-x-2"
        >
          <Plus className="h-4 w-4" />
          <span>Add New Class</span>
        </Button>
      </div>

      {Array.isArray(classes) && classes.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4 sm:gap-6">
          {classes.map((classItem: ClassItem) => (
            <Card
              key={classItem.id}
              className="hover:shadow-lg transition-shadow"
            >
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Book className="h-5 w-5 text-primary-600" />
                  <span className="text-sm sm:text-base">
                    {classItem.name} {classItem.section}
                  </span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Class Teacher:</span>
                  <span className="text-sm font-medium">
                    {classItem.classTeacher?.name ?? "N/A"}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Total Students:</span>
                  <span className="text-sm font-medium">
                    {students.filter(
                      (student) => student.classId === classItem.id
                    ).length ?? "N/A"}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Attendance:</span>
                  <span className="text-sm font-medium">{"N/A"}</span>
                </div>
                <div className="pt-3 space-y w-full">
                  <div className="flex gap-2 w-full">
                    <TooltipProvider>
                      <div className="flex gap-2 w-full">
                        {/* Edit */}
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => openModal("edit", classItem)}
                              className="w-1/4 justify-center"
                            >
                              <Pencil className="w-4 h-4 mr-1" />
                            </Button>
                          </TooltipTrigger>
                          <TooltipContent>Edit class details</TooltipContent>
                        </Tooltip>

                        {/* View */}
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => openModal("view", classItem)}
                              className="w-1/4 justify-center"
                            >
                              <Eye className="w-4 h-4 mr-1" />
                            </Button>
                          </TooltipTrigger>
                          <TooltipContent>
                            View class information
                          </TooltipContent>
                        </Tooltip>

                        {/* Attendance (Link) */}
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <Button
                              asChild
                              variant="outline"
                              size="sm"
                              className="w-1/4 p-0"
                            >
                              <Link
                                to="/dashboard/attendance"
                                state={{ classId: classItem.id }}
                                className="flex w-full h-full items-center justify-center px-2"
                              >
                                <CalendarCheck className="w-4 h-4 mr-1" />
                              </Link>
                            </Button>
                          </TooltipTrigger>
                          <TooltipContent>
                            Mark or view attendance
                          </TooltipContent>
                        </Tooltip>

                        {/* Delete */}
                        <Tooltip>
                          <TooltipTrigger asChild>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => openModal("delete", classItem)}
                              className="w-1/4 justify-center"
                            >
                              <Trash2 className="w-4 h-4 mr-1 text-red-500" />
                            </Button>
                          </TooltipTrigger>
                          <TooltipContent>Delete this class</TooltipContent>
                        </Tooltip>
                      </div>
                    </TooltipProvider>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <EmptyState
          icon={<GraduationCap className="h-12 w-12 text-primary-700" />}
          title="No Classes Found"
          description="🚀 It looks like you haven’t added any classes yet. Start by creating your first class."
          buttonText="Add New Class"
          onClick={() => openModal("add")}
        />
      )}

      {modal === "add" && (
        <AddClassPopup
          isOpen={isOpen}
          onClose={closeModal}
          onSubmit={handleAdd}
        />
      )}
      {modal === "edit" && selectedClass && (
        <EditClassPopup
          isOpen={isOpen}
          classData={selectedClass}
          onClose={closeModal}
          onSubmit={handleEdit}
        />
      )}
      {modal === "view" && selectedClass && (
        <ViewClassPopup
          isOpen={isOpen}
          classData={selectedClass}
          onClose={closeModal}
        />
      )}
      {modal === "delete" && selectedClass && (
        <DeleteClassPopup
          isOpen={isOpen}
          classData={selectedClass}
          onClose={closeModal}
          onConfirm={() => handleDelete(selectedClass.id)}
        />
      )}
    </div>
  );
};

export default Classes;
