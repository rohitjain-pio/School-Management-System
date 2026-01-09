
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { DollarSign, Calendar, Users, Building } from 'lucide-react';

const Payment: React.FC = () => {
  return (
    <div className="space-y-6">
      <div className="text-center py-16">
        <div className="mx-auto w-24 h-24 bg-red-100 rounded-full flex items-center justify-center mb-6">
          <DollarSign className="h-12 w-12 text-red-500" />
        </div>
        
        <h2 className="text-3xl font-bold text-gray-900 mb-4">Payment & Salary Management</h2>
        <p className="text-gray-600 text-lg mb-8 max-w-2xl mx-auto">
          This comprehensive payment system will handle all financial transactions, salary management, 
          and payment tracking for your educational institution.
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
              <DollarSign className="h-5 w-5" />
              <span>Salary Processing</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Automated salary calculations and disbursements</p>
          </CardContent>
        </Card>

        <Card className="border-dashed border-2 border-red-200 bg-red-50">
          <CardHeader>
            <CardTitle className="text-red-600 flex items-center space-x-2">
              <Users className="h-5 w-5" />
              <span>Fee Management</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Student fee collection and tracking</p>
          </CardContent>
        </Card>

        <Card className="border-dashed border-2 border-red-200 bg-red-50">
          <CardHeader>
            <CardTitle className="text-red-600 flex items-center space-x-2">
              <Calendar className="h-5 w-5" />
              <span>Payment Schedules</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Flexible payment scheduling options</p>
          </CardContent>
        </Card>

        <Card className="border-dashed border-2 border-red-200 bg-red-50">
          <CardHeader>
            <CardTitle className="text-red-600 flex items-center space-x-2">
              <Building className="h-5 w-5" />
              <span>Financial Reports</span>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">Comprehensive financial analytics</p>
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
              <h4 className="font-semibold text-gray-900">Payment Processing</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>• Multiple payment gateway integration</li>
                <li>• Automated recurring payments</li>
                <li>• Payment reminders and notifications</li>
                <li>• Digital receipts and invoices</li>
              </ul>
            </div>
            <div className="space-y-3">
              <h4 className="font-semibold text-gray-900">Salary Management</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>• Payroll processing automation</li>
                <li>• Tax calculations and deductions</li>
                <li>• Salary slip generation</li>
                <li>• Bank transfer integration</li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default Payment;
