
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { LayoutDashboard, Users, Book, Calendar } from 'lucide-react';

const Reports: React.FC = () => {
  const reportTypes = [
    {
      title: 'Student Performance',
      description: 'Comprehensive analysis of student grades and progress',
      icon: Users,
      lastGenerated: '2 days ago',
    },
    {
      title: 'Class Attendance',
      description: 'Attendance reports for all classes and time periods',
      icon: Calendar,
      lastGenerated: '1 week ago',
    },
    {
      title: 'Course Analytics',
      description: 'Course completion rates and student engagement metrics',
      icon: Book,
      lastGenerated: '3 days ago',
    },
    {
      title: 'Teacher Performance',
      description: 'Teaching effectiveness and student feedback analysis',
      icon: LayoutDashboard,
      lastGenerated: '1 month ago',
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold text-gray-900">Reports</h2>
        <p className="text-gray-600 mt-2">Generate and view various analytical reports</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {reportTypes.map((report, index) => {
          const Icon = report.icon;
          return (
            <Card key={index} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <CardTitle className="flex items-center space-x-3">
                  <div className="p-2 bg-primary-100 rounded-lg">
                    <Icon className="h-6 w-6 text-primary-600" />
                  </div>
                  <span>{report.title}</span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-gray-600 text-sm">{report.description}</p>
                <div className="flex items-center justify-between">
                  <span className="text-xs text-gray-500">Last generated: {report.lastGenerated}</span>
                </div>
                <div className="flex space-x-2">
                  <Button className="flex-1">Generate Report</Button>
                  <Button variant="outline" className="flex-1">View History</Button>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Quick Stats</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="text-center p-4 bg-blue-50 rounded-lg">
              <div className="text-2xl font-bold text-blue-600">92%</div>
              <div className="text-sm text-blue-800">Average Attendance</div>
            </div>
            <div className="text-center p-4 bg-green-50 rounded-lg">
              <div className="text-2xl font-bold text-green-600">87%</div>
              <div className="text-sm text-green-800">Course Completion</div>
            </div>
            <div className="text-center p-4 bg-yellow-50 rounded-lg">
              <div className="text-2xl font-bold text-yellow-600">4.2/5</div>
              <div className="text-sm text-yellow-800">Student Satisfaction</div>
            </div>
            <div className="text-center p-4 bg-purple-50 rounded-lg">
              <div className="text-2xl font-bold text-purple-600">95%</div>
              <div className="text-sm text-purple-800">Teacher Efficiency</div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default Reports;
