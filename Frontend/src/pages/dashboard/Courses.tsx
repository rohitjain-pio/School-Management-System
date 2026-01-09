
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Plus, Book, Users, Calendar } from 'lucide-react';

const Courses: React.FC = () => {
  const courses = [
    {
      id: 1,
      name: 'Advanced Mathematics',
      code: 'MATH-401',
      instructor: 'Dr. Sarah Smith',
      students: 25,
      duration: '16 weeks',
      status: 'Active',
    },
    {
      id: 2,
      name: 'Modern Physics',
      code: 'PHYS-301',
      instructor: 'Prof. John Johnson',
      students: 18,
      duration: '14 weeks',
      status: 'Active',
    },
    {
      id: 3,
      name: 'Creative Writing',
      code: 'ENG-201',
      instructor: 'Ms. Emily Davis',
      students: 30,
      duration: '12 weeks',
      status: 'Starting Soon',
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-bold text-gray-900">Courses</h2>
          <p className="text-gray-600 mt-2">Manage course curriculum and content</p>
        </div>
        <Button className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Create New Course</span>
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {courses.map((course) => (
          <Card key={course.id} className="hover:shadow-lg transition-shadow">
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <Book className="h-5 w-5 text-primary-600" />
                  <span>{course.name}</span>
                </div>
                <span className={`px-2 py-1 text-xs rounded-full ${
                  course.status === 'Active' 
                    ? 'bg-green-100 text-green-800' 
                    : 'bg-yellow-100 text-yellow-800'
                }`}>
                  {course.status}
                </span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="text-sm text-gray-600">
                Course Code: <span className="font-mono font-medium">{course.code}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">Instructor:</span>
                <span className="text-sm font-medium">{course.instructor}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">Students:</span>
                <div className="flex items-center space-x-1">
                  <Users className="h-4 w-4 text-gray-500" />
                  <span className="text-sm font-medium">{course.students}</span>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600">Duration:</span>
                <div className="flex items-center space-x-1">
                  <Calendar className="h-4 w-4 text-gray-500" />
                  <span className="text-sm font-medium">{course.duration}</span>
                </div>
              </div>
              <div className="pt-3 flex space-x-2">
                <Button variant="outline" size="sm" className="flex-1">
                  Edit Course
                </Button>
                <Button variant="outline" size="sm" className="flex-1">
                  View Details
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
};

export default Courses;
