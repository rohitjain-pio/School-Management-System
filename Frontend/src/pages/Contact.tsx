import { useState } from "react";
import { Mail, Phone, MapPin, Clock, Send } from "lucide-react";


interface ContactInfo {
  icon: React.ComponentType<{ className?: string }>;
  title: string;
  details: string[];
}

interface FormData {
  name: string;
  email: string;
  school: string;
  students: string;
  message: string;
}

const Contact: React.FC = () => {
  const [formData, setFormData] = useState<FormData>({
    name: "",
    email: "",
    school: "",
    students: "",
    message: "",
  });

  const contactInfo: ContactInfo[] = [
    {
      icon: Mail,
      title: "Email Us",
      details: ["avrtech25@outlook.com", "work.rohitjain@outlook.com"],
    },
    {
      icon: Phone,
      title: "Call Us",
      details: ["+91 7014791203", "+91 9171212231"],
    },
    {
      icon: MapPin,
      title: "Visit Us",
      details: [
        "AVR Tech, Business Corner",
        " VT Road, Mansarover, Jaipur, Rajasthan.",
      ],
    },
    {
      icon: Clock,
      title: "Business Hours",
      details: ["Everyday: 10AM - 6PM", "Support: 24/7"],
    },
  ];

  const handleInputChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    // Handle form submission here
    console.log("Form submitted:", formData);
    // Reset form
    setFormData({
      name: "",
      email: "",
      school: "",
      students: "",
      message: "",
    });
  };

  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="relative bg-gradient-to-br from-primary-50 via-white to-blue-50 py-20 lg:py-32 overflow-hidden">
        <div className="absolute inset-0 opacity-30">
          <div className="absolute top-20 left-10 w-96 h-96 bg-primary-200 rounded-full mix-blend-multiply filter blur-3xl animate-pulse"></div>
          <div
            className="absolute bottom-20 right-10 w-96 h-96 bg-blue-200 rounded-full mix-blend-multiply filter blur-3xl animate-pulse"
            style={{ animationDelay: "2s" }}
          ></div>
        </div>

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h1 className="text-4xl lg:text-6xl font-bold text-gray-900 leading-tight mb-8">
            Get in
            <span className="bg-gradient-to-r from-primary-600 to-blue-600 bg-clip-text text-transparent">
              {" "}
              Touch
            </span>
          </h1>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto mb-12 leading-relaxed">
            Ready to transform your school's management system? We're here to
            help you get started and answer any questions you might have.
          </p>
        </div>
      </section>

      {/* Contact Info */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8 mb-16">
            {contactInfo.map((info, index) => {
              const Icon = info.icon;
              return (
                <div
                  key={index}
                  className="text-center group bg-gradient-to-br from-gray-50 to-white p-8 rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 border border-gray-100"
                >
                  <div className="w-16 h-16 bg-gradient-to-br from-primary-500 to-primary-700 rounded-2xl flex items-center justify-center mx-auto mb-6 group-hover:scale-110 transition-transform duration-300">
                    <Icon className="w-8 h-8 text-white" />
                  </div>
                  <h3 className="text-xl font-bold text-gray-900 mb-4">
                    {info.title}
                  </h3>
                  {info.details.map((detail, detailIndex) => (
                    <p key={detailIndex} className="text-gray-600 mb-1">
                      {detail}
                    </p>
                  ))}
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Contact Form */}
      <section className="py-20 bg-gradient-to-br from-gray-50 to-blue-50">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-3xl shadow-2xl overflow-hidden">
            <div className="grid lg:grid-cols-2">
              {/* Form */}
              <div className="p-8 lg:p-12">
                <h2 className="text-3xl font-bold text-gray-900 mb-6">
                  Send us a message
                </h2>
                <form onSubmit={handleSubmit} className="space-y-6">
                  <div>
                    <label
                      htmlFor="name"
                      className="block text-sm font-medium text-gray-700 mb-2"
                    >
                      Full Name *
                    </label>
                    <input
                      type="text"
                      id="name"
                      name="name"
                      value={formData.name}
                      onChange={handleInputChange}
                      required
                      className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-300"
                      placeholder="Enter your full name"
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="email"
                      className="block text-sm font-medium text-gray-700 mb-2"
                    >
                      Email Address *
                    </label>
                    <input
                      type="email"
                      id="email"
                      name="email"
                      value={formData.email}
                      onChange={handleInputChange}
                      required
                      className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-300"
                      placeholder="Enter your email address"
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="school"
                      className="block text-sm font-medium text-gray-700 mb-2"
                    >
                      School/Institution
                    </label>
                    <input
                      type="text"
                      id="school"
                      name="school"
                      value={formData.school}
                      onChange={handleInputChange}
                      className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-300"
                      placeholder="Enter your school name"
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="students"
                      className="block text-sm font-medium text-gray-700 mb-2"
                    >
                      Number of Students
                    </label>
                    <select
                      id="students"
                      name="students"
                      value={formData.students}
                      onChange={handleInputChange}
                      className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-300"
                    >
                      <option value="">Select range</option>
                      <option value="1-50">1-50 students</option>
                      <option value="51-200">51-200 students</option>
                      <option value="201-500">201-500 students</option>
                      <option value="501-1000">501-1000 students</option>
                      <option value="1000+">1000+ students</option>
                    </select>
                  </div>

                  <div>
                    <label
                      htmlFor="message"
                      className="block text-sm font-medium text-gray-700 mb-2"
                    >
                      Message *
                    </label>
                    <textarea
                      id="message"
                      name="message"
                      value={formData.message}
                      onChange={handleInputChange}
                      required
                      rows={5}
                      className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-300 resize-none"
                      placeholder="Tell us about your needs and how we can help..."
                    />
                  </div>

                  <button
                    type="submit"
                    className="w-full bg-gradient-to-r from-primary-600 to-primary-700 text-white px-8 py-4 rounded-xl font-bold hover:primary-700 hover:to-primary-800 transition-all duration-300 transform hover:scale-105 shadow-lg hover:shadow-xl flex items-center justify-center"
                  >
                    <Send className="w-5 h-5 mr-2" />
                    Send Message
                  </button>
                </form>
              </div>

              {/* Contact Info Sidebar */}
              <div className=" bg-primary-600 p-8 lg:p-12 text-white">
                <h3 className="text-2xl font-bold mb-8">
                  Let's Start a Conversation
                </h3>
                <div className="space-y-6">
                  <div>
                    <h4 className="font-semibold mb-2">Quick Response</h4>
                    <p className="text-primary-100">
                      We typically respond to all inquiries within 24 hours
                      during business days.
                    </p>
                  </div>
                  <div>
                    <h4 className="font-semibold mb-2">Free Consultation</h4>
                    <p className="text-primary-100">
                      Schedule a free 30-minute consultation to discuss your
                      school's specific needs.
                    </p>
                  </div>
                  <div>
                    <h4 className="font-semibold mb-2">Custom Demo</h4>
                    <p className="text-primary-100">
                      Get a personalized demo tailored to your institution's
                      requirements.
                    </p>
                  </div>
                </div>

                <div className="mt-12 pt-8 border-t border-primary-400">
                  <h4 className="font-semibold mb-4">Prefer to call?</h4>
                  <p className="text-2xl font-bold text-white">
                    +91 7014791203
                  </p>
                  <p className="text-primary-100 mt-2">
                    Available Everyday, 10AM-6PM IST
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="py-20 bg-white">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl lg:text-5xl font-bold text-gray-900 mb-6">
              Quick
              <span className="bg-gradient-to-r from-primary-600 to-blue-600 bg-clip-text text-transparent">
                {" "}
                Answers
              </span>
            </h2>
            <p className="text-xl text-gray-600">
              Common questions we receive from schools and educators.
            </p>
          </div>

          <div className="space-y-6">
            <div className="bg-gradient-to-r from-gray-50 to-blue-50 p-8 rounded-2xl">
              <h3 className="text-xl font-bold text-gray-900 mb-4">
                How long does implementation take?
              </h3>
              <p className="text-gray-600">
                Most schools are up and running within 1-2 weeks. We provide
                full onboarding support and training for your staff.
              </p>
            </div>

            <div className="bg-gradient-to-r from-gray-50 to-blue-50 p-8 rounded-2xl">
              <h3 className="text-xl font-bold text-gray-900 mb-4">
                Can you import our existing data?
              </h3>
              <p className="text-gray-600">
                Yes! We can import student records, grades, and other data from
                most common school management systems. Our team handles the
                migration process.
              </p>
            </div>

            <div className="bg-gradient-to-r from-gray-50 to-blue-50 p-8 rounded-2xl">
              <h3 className="text-xl font-bold text-gray-900 mb-4">
                Do you offer training for our staff?
              </h3>
              <p className="text-gray-600">
                Absolutely. We provide comprehensive training sessions, video
                tutorials, and ongoing support to ensure your team feels
                confident using EduManage.
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default Contact;
