const UnauthorizedPage = () => (
  <div className="flex flex-col items-center justify-center h-screen">
    <h1 className="text-3xl font-bold mb-4">Unauthorized</h1>
    <p className="text-gray-600 mb-6">You do not have permission to view this page.</p>
    <button
      onClick={() => window.history.back()}
      className="px-4 py-2 bg-primary-600 text-white rounded"
    >
      Go Back
    </button>
  </div>
);

export default UnauthorizedPage;
