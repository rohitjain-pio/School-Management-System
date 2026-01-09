import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Users,
  Book,
  Calendar,
  LayoutDashboard,
  UserCheck,
  Clock,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { useDashboardHome } from "@/hooks/useDashboardHome";
import DashboardHomeSkeleton from "@/skeletons/DashboardHomeSkeleton";

const DashboardHome: React.FC = () => {
  const stats = [
    { title: "Total Teachers", value: "45", icon: Users },
    { title: "Present Teachers", value: "42", icon: UserCheck },
    { title: "Total Students", value: "67", icon: Users },
    { title: "Present Students", value: "60", icon: UserCheck },
  ];

  const upcomingFeatures = [
    {
      title: "Bio Matrix Attendance",
      icon: UserCheck,
      description: "Automated attendance with biometric data",
    },
    {
      title: "Class & Exam Schedule",
      icon: Clock,
      description: "Advanced scheduling with notifications",
    },
  ];

  const currentDate = new Date().toLocaleDateString("en-US", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  // const { data: stats, isLoading, error } = useDashboardHome();

  // if (isLoading) return <DashboardHomeSkeleton />;
  // if (error) return <div>Error loading classes: {error.message}</div>;

  return (
    <div className="container mx-auto px-4  space-y-8">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Welcome back!</h1>
          <p className="text-gray-600 mt-2 text-sm">
            Today's date: {currentDate} | Here's what's happening at your school
            today.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <Card
              key={stat.title}
              className="hover:shadow-lg transition-shadow duration-300"
            >
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-gray-600">
                  {stat.title}
                </CardTitle>
                <Icon
                  className={`h-5 w-5 ${
                    stat.icon === Users ? "text-blue-600" : "text-green-600"
                  }`}
                />
              </CardHeader>
              <CardContent>
                <div
                  className={`text-2xl font-bold ${
                    stat.title.includes("Total")
                      ? "text-blue-600"
                      : "text-green-600"
                  }`}
                >
                  {stat.value}
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <div className="mb-6">
        <h3 className="text-xl font-semibold text-gray-900 mb-4">
          Upcoming Features
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {upcomingFeatures.map((feature) => {
            const Icon = feature.icon;
            return (
              <Card
                key={feature.title}
                className="border-2 border-dashed border-red-200 bg-red-50 hover:border-red-300 transition-colors duration-300"
              >
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-4">
                      <Icon className="h-6 w-6 text-red-500" />
                      <div>
                        <h4 className="font-medium text-gray-900">
                          {feature.title}
                        </h4>
                        <p className="text-sm text-gray-600">
                          {feature.description}
                        </p>
                      </div>
                    </div>
                    <span className="text-xs bg-red-100 text-red-800 px-2 py-1 rounded-full">
                      Upcoming
                    </span>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg font-semibold text-gray-900">
              Recent Activities
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex items-center space-x-3">
                <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-gray-900">
                    New student enrolled
                  </p>
                  <p className="text-xs text-gray-500">2 minutes ago</p>
                </div>
              </div>
              <div className="flex items-center space-x-3">
                <div className="w-2 h-2 bg-green-500 rounded-full"></div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-gray-900">
                    Class assignment submitted
                  </p>
                  <p className="text-xs text-gray-500">5 minutes ago</p>
                </div>
              </div>
              <div className="flex items-center space-x-3">
                <div className="w-2 h-2 bg-yellow-500 rounded-full"></div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-gray-900">
                    Teacher schedule updated
                  </p>
                  <p className="text-xs text-gray-500">15 minutes ago</p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg font-semibold text-gray-900">
              Upcoming Events
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="border-l-4 border-blue-500 pl-4">
                <p className="text-sm font-medium text-gray-900">
                  Parent-Teacher Meeting
                </p>
                <p className="text-xs text-gray-500">Tomorrow, 2:00 PM</p>
              </div>
              <div className="border-l-4 border-green-500 pl-4">
                <p className="text-sm font-medium text-gray-900">
                  Science Fair
                </p>
                <p className="text-xs text-gray-500">Friday, 10:00 AM</p>
              </div>
              <div className="border-l-4 border-purple-500 pl-4">
                <p className="text-sm font-medium text-gray-900">Exam Week</p>
                <p className="text-xs text-gray-500">Next Monday</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default DashboardHome;
