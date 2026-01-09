
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { FileText, Calendar, Users, CheckCircle } from 'lucide-react';

const Leave: React.FC = () => {
  return (
    <div className="space-y-6">
      <div className="text-center py-16">
        <div className="mx-auto w-24 h-24 bg-red-100 rounded-full flex items-center justify-center mb-6">
          <FileText className="h-12 w-12 text-red-500" />
        </div>
        
        <h2 className="text-3xl font-bold text-gray-900 mb-4">Leave Management System</h2>
        <p className="text-gray-600 text-lg mb-8 max-w-2xl mx-auto">
          A comprehensive leave management system to handle all leave applications, approvals, 
          and tracking for teachers, staff, and students.
        </p>
        
        <div className="inline-flex items-center space-x-2 bg-red-500 text-white px-4 py-2 rounded-full">
          <span className="text-sm font-medium">Coming Soon</span>
        </div>
      </div>

      {/* Feature Preview */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card className="border-dashed border-2 border-red-200 bg-red-50">
          <CardHeader>
            <CardTitle className="text-red-600 flex items-center space-x-2">
              <FileText className="h-5 w-5" />
              <span>Leave Applications</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Digital leave application submission</p>
          </CardContent>
        </Card>

        <Card className="border-dashed border-2 border-red-200 bg-red-50">
          <CardHeader>
            <CardTitle className="text-red-600 flex items-center space-x-2">
              <CheckCircle className="h-5 w-5" />
              <span>Approval Workflow</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Multi-level approval processes</p>
          </CardContent>
        </Card>

        <Card className="border-dashed border-2 border-red-200 bg-red-50">
          <CardHeader>
            <CardTitle className="text-red-600 flex items-center space-x-2">
              <Calendar className="h-5 w-5" />
              <span>Leave Calendar</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Visual leave tracking calendar</p>
          </CardContent>
        </Card>

        <Card className="border-dashed border-2 border-red-200 bg-red-50">
          <CardHeader>
            <CardTitle className="text-red-600 flex items-center space-x-2">
              <Users className="h-5 w-5" />
              <span>Balance Tracker</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Leave balance management</p>
          </CardContent>
        </Card>
      </div>

      {/* Key Features List */}
      <Card>
        <CardHeader>
          <CardTitle>Upcoming Features</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-3">
              <h4 className="font-semibold text-gray-900">Application Management</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>• Online leave application forms</li>
                <li>• Document attachment support</li>
                <li>• Automated email notifications</li>
                <li>• Leave history tracking</li>
              </ul>
            </div>
            <div className="space-y-3">
              <h4 className="font-semibold text-gray-900">Administrative Tools</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>• Bulk approval capabilities</li>
                <li>• Leave policy configuration</li>
                <li>• Reporting and analytics</li>
                <li>• Integration with payroll</li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default Leave;
