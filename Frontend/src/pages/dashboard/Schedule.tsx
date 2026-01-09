
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Calendar, Clock, Plus } from 'lucide-react';

const Schedule: React.FC = () => {
  const scheduleData = [
    {
      time: '9:00 AM',
      monday: { class: 'Math 101', room: 'A-101', teacher: 'Dr. Smith' },
      tuesday: { class: 'Physics', room: 'B-203', teacher: 'Prof. Johnson' },
      wednesday: { class: 'English', room: 'C-105', teacher: 'Ms. Davis' },
      thursday: { class: 'Math 101', room: 'A-101', teacher: 'Dr. Smith' },
      friday: { class: 'Chemistry', room: 'D-301', teacher: 'Dr. Wilson' },
    },
    {
      time: '10:30 AM',
      monday: { class: 'Physics', room: 'B-203', teacher: 'Prof. Johnson' },
      tuesday: { class: 'English', room: 'C-105', teacher: 'Ms. Davis' },
      wednesday: { class: 'Math 101', room: 'A-101', teacher: 'Dr. Smith' },
      thursday: { class: 'Chemistry', room: 'D-301', teacher: 'Dr. Wilson' },
      friday: { class: 'Physics', room: 'B-203', teacher: 'Prof. Johnson' },
    },
  ];

  const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'];

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-bold text-gray-900">Schedule</h2>
          <p className="text-gray-600 mt-2">View and manage class schedules</p>
        </div>
        <Button className="flex items-center space-x-2">
          <Plus className="h-4 w-4" />
          <span>Add Schedule</span>
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center space-x-2">
            <Calendar className="h-5 w-5" />
            <span>Weekly Schedule</span>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Time
                  </th>
                  {days.map((day) => (
                    <th key={day} className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      {day}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {scheduleData.map((timeSlot, index) => (
                  <tr key={index}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <Clock className="h-4 w-4 text-gray-500 mr-2" />
                        <span className="text-sm font-medium text-gray-900">{timeSlot.time}</span>
                      </div>
                    </td>
                    {days.map((day) => {
                      const dayKey = day.toLowerCase() as keyof typeof timeSlot;
                      const classInfo = timeSlot[dayKey] as any;
                      return (
                        <td key={day} className="px-6 py-4 whitespace-nowrap">
                          {classInfo && (
                            <div className="bg-primary-50 p-3 rounded-lg border border-primary-200">
                              <div className="text-sm font-medium text-primary-900">{classInfo.class}</div>
                              <div className="text-xs text-primary-600">{classInfo.room}</div>
                              <div className="text-xs text-primary-600">{classInfo.teacher}</div>
                            </div>
                          )}
                        </td>
                      );
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default Schedule;
