
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Settings as SettingsIcon, Users, Bell, Book, Calendar } from 'lucide-react';

const Settings: React.FC = () => {
  const settingSections = [
    {
      title: 'General Settings',
      description: 'Basic school information and preferences',
      icon: SettingsIcon,
      options: ['School Information', 'Academic Year Settings', 'Time Zone', 'Language Preferences'],
    },
    {
      title: 'User Management',
      description: 'Manage users, roles, and permissions',
      icon: Users,
      options: ['User Roles', 'Access Permissions', 'Account Settings', 'Password Policies'],
    },
    {
      title: 'Notifications',
      description: 'Configure system notifications and alerts',
      icon: Bell,
      options: ['Email Notifications', 'SMS Settings', 'Push Notifications', 'Alert Preferences'],
    },
    {
      title: 'Academic Settings',
      description: 'Course and grading system configuration',
      icon: Book,
      options: ['Grading Scale', 'Course Categories', 'Assessment Types', 'Report Card Settings'],
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold text-gray-900">Settings</h2>
        <p className="text-gray-600 mt-2">Configure your school management system</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {settingSections.map((section, index) => {
          const Icon = section.icon;
          return (
            <Card key={index} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <CardTitle className="flex items-center space-x-3">
                  <div className="p-2 bg-primary-100 rounded-lg">
                    <Icon className="h-6 w-6 text-primary-600" />
                  </div>
                  <span>{section.title}</span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-gray-600 text-sm">{section.description}</p>
                <div className="space-y-2">
                  {section.options.map((option, optionIndex) => (
                    <div key={optionIndex} className="flex items-center justify-between p-2 hover:bg-gray-50 rounded">
                      <span className="text-sm">{option}</span>
                      <Button variant="ghost" size="sm">Configure</Button>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>System Information</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <h4 className="font-medium text-gray-900 mb-2">System Status</h4>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span>Version:</span>
                  <span>v2.1.0</span>
                </div>
                <div className="flex justify-between">
                  <span>Last Updated:</span>
                  <span>2 days ago</span>
                </div>
                <div className="flex justify-between">
                  <span>Status:</span>
                  <span className="text-green-600">Operational</span>
                </div>
              </div>
            </div>
            <div>
              <h4 className="font-medium text-gray-900 mb-2">Storage</h4>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span>Used Space:</span>
                  <span>2.1 GB</span>
                </div>
                <div className="flex justify-between">
                  <span>Available:</span>
                  <span>7.9 GB</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div className="bg-primary-600 h-2 rounded-full" style={{ width: '21%' }}></div>
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default Settings;
