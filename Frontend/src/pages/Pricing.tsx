import { Link } from "react-router-dom";
import { CheckCircle, X } from "lucide-react";


interface PricingFeature {
  name: string;
  basic: boolean;
  professional: boolean;
  enterprise: boolean | string;
}

interface PricingPlan {
  name: string;
  price: string;
  period: string;
  description: string;
  features: string[];
  popular?: boolean;
  cta: string;
  ctaLink: string;
}

const Pricing: React.FC = () => {
  const pricingPlans: PricingPlan[] = [
    {
      name: "Basic",
      price: "$29",
      period: "per month",
      description:
        "Perfect for small schools getting started with digital management.",
      features: [
        "Up to 200 students",
        "Basic student profiles",
        "Grade management",
        "Email support",
        "5GB storage",
      ],
      cta: "Start Free Trial",
      ctaLink: "/contact",
    },
    {
      name: "Professional",
      price: "$79",
      period: "per month",
      description:
        "Comprehensive solution for growing educational institutions.",
      features: [
        "Up to 1,000 students",
        "Advanced analytics",
        "Parent portal access",
        "Priority support",
        "50GB storage",
        "Custom reports",
        "API access",
      ],
      popular: true,
      cta: "Start Free Trial",
      ctaLink: "/contact",
    },
    {
      name: "Enterprise",
      price: "Custom",
      period: "pricing",
      description:
        "Tailored solutions for large school districts and universities.",
      features: [
        "Unlimited students",
        "Advanced integrations",
        "Dedicated support",
        "Custom training",
        "Unlimited storage",
        "White-label options",
        "SLA guarantee",
      ],
      cta: "Contact Sales",
      ctaLink: "/contact",
    },
  ];

  const detailedFeatures: PricingFeature[] = [
    {
      name: "Attendance Tracking",
      basic: true,
      professional: true,
      enterprise: true,
    },
    {
      name: "Class Schedule",
      basic: true,
      professional: true,
      enterprise: true,
    },
    {
      name: "Course Management",
      basic: true,
      professional: true,
      enterprise: true,
    },
    {
      name: "Multi-device Compatibility",
      basic: true,
      professional: true,
      enterprise: true,
    },
    {
      name: "Secure & Reliable Infrastructure",
      basic: true,
      professional: true,
      enterprise: true,
    },
    {
      name: "Student Management",
      basic: true,
      professional: true,
      enterprise: true,
    },
    {
      name: "Accounts Management",
      basic: false,
      professional: true,
      enterprise: true,
    },
    {
      name: "Advanced Analytics",
      basic: false,
      professional: true,
      enterprise: true,
    },
    {
      name: "Announcements & Events",
      basic: false,
      professional: true,
      enterprise: true,
    },
    {
      name: "Parent Portal",
      basic: false,
      professional: true,
      enterprise: true,
    },
    { name: "Reports", basic: false, professional: true, enterprise: true },
    {
      name: "Role-based Access Control",
      basic: false,
      professional: true,
      enterprise: true,
    },
    {
      name: "Dedicated Support",
      basic: false,
      professional: false,
      enterprise: true,
    },
    {
      name: "Custom Integrations",
      basic: false,
      professional: false,
      enterprise: "Available",
    },
  ];

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
            Simple, Transparent
            <span className="bg-gradient-to-r from-primary-600 to-blue-600 bg-clip-text text-transparent">
              {" "}
              Pricing
            </span>
          </h1>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto mb-12 leading-relaxed">
            Choose the perfect plan for your school. Start with a free 30-day
            trial, no credit card required. Upgrade or downgrade at any time.
          </p>
          <div className="inline-flex items-center bg-white/80 backdrop-blur-sm rounded-2xl p-2 shadow-lg border border-gray-100">
            <div className="bg-primary-600 text-white px-6 py-3 rounded-xl font-medium">
              Monthly
            </div>
            <div className="text-gray-700 px-6 py-3 font-medium">
              Yearly (Save 20%)
            </div>
          </div>
        </div>
      </section>

      {/* Pricing Cards */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-3 gap-8">
            {pricingPlans.map((plan, index) => (
              <div
                key={index}
                className={`relative bg-white rounded-2xl shadow-lg border-2 transition-all duration-300 transform hover:-translate-y-2 ${
                  plan.popular
                    ? "border-primary-500 shadow-2xl scale-105"
                    : "border-gray-200 hover:border-primary-300"
                }`}
              >
                {plan.popular && (
                  <div className="absolute -top-4 left-1/2 transform -translate-x-1/2">
                    <div className="bg-gradient-to-r from-primary-600 to-primary-700 text-white px-6 py-2 rounded-full text-sm font-bold shadow-lg">
                      Most Popular
                    </div>
                  </div>
                )}

                <div className="p-8">
                  <h3 className="text-2xl font-bold text-gray-900 mb-4">
                    {plan.name}
                  </h3>
                  <div className="mb-6">
                    <span className="text-4xl font-bold text-gray-900">
                      {plan.price}
                    </span>
                    {plan.price !== "Custom" && (
                      <span className="text-gray-600 ml-2">{plan.period}</span>
                    )}
                  </div>
                  <p className="text-gray-600 mb-8">{plan.description}</p>

                  <ul className="space-y-4 mb-8">
                    {plan.features.map((feature, featureIndex) => (
                      <li key={featureIndex} className="flex items-center">
                        <CheckCircle className="w-5 h-5 text-green-500 mr-3 flex-shrink-0" />
                        <span className="text-gray-700">{feature}</span>
                      </li>
                    ))}
                  </ul>

                  <Link
                    to={plan.ctaLink}
                    className={`block w-full text-center py-4 px-6 rounded-xl font-bold transition-all duration-300 transform hover:scale-105 ${
                      plan.popular
                        ? "bg-gradient-to-r from-primary-600 to-primary-700 text-white hover:from-primary-700 hover:to-primary-800 shadow-lg"
                        : "border-2 border-primary-600 text-primary-600 hover:bg-primary-50"
                    }`}
                  >
                    {plan.cta}
                  </Link>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Feature Comparison */}
      <section className="py-20 bg-gradient-to-br from-gray-50 to-blue-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl lg:text-5xl font-bold text-gray-900 mb-6">
              Compare
              <span className="bg-gradient-to-r from-primary-600 to-blue-600 bg-clip-text text-transparent">
                {" "}
                Features
              </span>
            </h2>
            <p className="text-xl text-gray-600 max-w-3xl mx-auto">
              See exactly what's included in each plan to make the best choice
              for your school.
            </p>
          </div>

          <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-4 text-left text-lg font-bold text-gray-900">
                      Features
                    </th>
                    <th className="px-6 py-4 text-center text-lg font-bold text-gray-900">
                      Basic
                    </th>
                    <th className="px-6 py-4 text-center text-lg font-bold text-primary-600">
                      Professional
                    </th>
                    <th className="px-6 py-4 text-center text-lg font-bold text-gray-900">
                      Enterprise
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {detailedFeatures.map((feature, index) => (
                    <tr
                      key={index}
                      className="hover:bg-gray-50 transition-colors duration-200"
                    >
                      <td className="px-6 py-4 font-medium text-gray-900">
                        {feature.name}
                      </td>
                      <td className="px-6 py-4 text-center">
                        {feature.basic ? (
                          <CheckCircle className="w-6 h-6 text-green-500 mx-auto" />
                        ) : (
                          <X className="w-6 h-6 text-gray-300 mx-auto" />
                        )}
                      </td>
                      <td className="px-6 py-4 text-center">
                        {feature.professional ? (
                          <CheckCircle className="w-6 h-6 text-green-500 mx-auto" />
                        ) : (
                          <X className="w-6 h-6 text-gray-300 mx-auto" />
                        )}
                      </td>
                      <td className="px-6 py-4 text-center">
                        {feature.enterprise === true ? (
                          <CheckCircle className="w-6 h-6 text-green-500 mx-auto" />
                        ) : feature.enterprise === false ? (
                          <X className="w-6 h-6 text-gray-300 mx-auto" />
                        ) : (
                          <span className="text-primary-600 font-medium">
                            {feature.enterprise}
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="py-20 bg-white">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl lg:text-5xl font-bold text-gray-900 mb-6">
              Frequently Asked
              <span className="bg-gradient-to-r from-primary-600 to-blue-600 bg-clip-text text-transparent">
                {" "}
                Questions
              </span>
            </h2>
          </div>

          <div className="space-y-8">
            <div className="bg-gradient-to-r from-gray-50 to-blue-50 p-8 rounded-2xl">
              <h3 className="text-xl font-bold text-gray-900 mb-4">
                Can I change plans anytime?
              </h3>
              <p className="text-gray-600">
                Yes, you can upgrade or downgrade your plan at any time. Changes
                take effect immediately, and we'll prorate any billing
                differences.
              </p>
            </div>

            <div className="bg-gradient-to-r from-gray-50 to-blue-50 p-8 rounded-2xl">
              <h3 className="text-xl font-bold text-gray-900 mb-4">
                Is there a free trial?
              </h3>
              <p className="text-gray-600">
                Absolutely! We offer a 30-day free trial with full access to all
                Professional features. No credit card required to start.
              </p>
            </div>

            <div className="bg-gradient-to-r from-gray-50 to-blue-50 p-8 rounded-2xl">
              <h3 className="text-xl font-bold text-gray-900 mb-4">
                What kind of support do you provide?
              </h3>
              <p className="text-gray-600">
                We provide email support for Basic plans, priority support for
                Professional plans, and dedicated support with phone access for
                Enterprise customers.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 bg-gradient-to-r from-primary-600 via-primary-700 to-blue-600 relative overflow-hidden">
        <div className="absolute inset-0 bg-black opacity-10"></div>
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl lg:text-5xl font-bold text-white mb-6">
            Ready to Get Started?
          </h2>
          <p className="text-xl text-blue-100 mb-10 max-w-2xl mx-auto">
            Join hundreds of schools already using EduManage to streamline their
            operations.
          </p>
          <Link
            to="/contact"
            className="bg-white text-primary-700 px-10 py-4 rounded-xl text-lg font-bold hover:bg-gray-50 transition-all duration-300 transform hover:scale-105 shadow-xl"
          >
            Start Your Free Trial
          </Link>
        </div>
      </section>
    </div>
  );
};

export default Pricing;
