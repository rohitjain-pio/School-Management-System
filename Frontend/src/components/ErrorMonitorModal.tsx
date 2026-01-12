import React, { useState, useMemo } from 'react';
import { useErrorMonitor, ErrorEntry } from '../context/ErrorMonitorContext';
import { geminiService } from '../lib/geminiService';
import { errorCache } from '../lib/errorCache';

interface ErrorMonitorModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const categories = ['All', 'Database', 'Backend', 'Frontend', 'Network', 'Validation', 'Auth'];

export const ErrorMonitorModal: React.FC<ErrorMonitorModalProps> = ({ isOpen, onClose }) => {
  const { errors, backendErrors, clearErrors, fetchBackendErrors, isPolling, togglePolling } = useErrorMonitor();
  const [activeTab, setActiveTab] = useState('All');
  const [searchQuery, setSearchQuery] = useState('');
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [analyzingId, setAnalyzingId] = useState<string | null>(null);
  const [aiSuggestions, setAiSuggestions] = useState<Record<string, { suggestion: string; cached: boolean }>>({});

  const allErrors = useMemo(() => {
    return [...errors, ...backendErrors].sort((a, b) => 
      new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
    );
  }, [errors, backendErrors]);

  const filteredErrors = useMemo(() => {
    let filtered = allErrors;

    // Filter by category
    if (activeTab !== 'All') {
      filtered = filtered.filter(e => e.category === activeTab);
    }

    // Filter by search query
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(e => 
        e.message.toLowerCase().includes(query) ||
        e.source?.toLowerCase().includes(query) ||
        e.category.toLowerCase().includes(query)
      );
    }

    return filtered;
  }, [allErrors, activeTab, searchQuery]);

  const exportErrors = () => {
    const dataStr = JSON.stringify(filteredErrors, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `errors-${new Date().toISOString()}.json`;
    link.click();
    URL.revokeObjectURL(url);
  };

  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      Database: 'bg-purple-100 text-purple-800',
      Backend: 'bg-red-100 text-red-800',
      Frontend: 'bg-blue-100 text-blue-800',
      Network: 'bg-orange-100 text-orange-800',
      Validation: 'bg-yellow-100 text-yellow-800',
      Auth: 'bg-pink-100 text-pink-800',
    };
    return colors[category] || 'bg-gray-100 text-gray-800';
  };

  const handleAnalyzeError = async (error: ErrorEntry) => {
    // Check cache first
    const cached = errorCache.getCachedSuggestion(error.category, error.message, error.stackTrace);
    if (cached) {
      setAiSuggestions(prev => ({
        ...prev,
        [error.id]: { suggestion: cached, cached: true }
      }));
      return;
    }

    setAnalyzingId(error.id);

    try {
      const response = await geminiService.analyzeError({
        category: error.category,
        message: error.message,
        stackTrace: error.stackTrace,
        metadata: error.metadata,
      });

      if (response.success && response.suggestion) {
        setAiSuggestions(prev => ({
          ...prev,
          [error.id]: { suggestion: response.suggestion!, cached: response.cached }
        }));

        // Cache the suggestion
        if (!response.cached) {
          errorCache.setCachedSuggestion(error.category, error.message, response.suggestion, error.stackTrace);
        }
      } else {
        // Show error message
        alert(response.message || 'Failed to analyze error');
      }
    } catch (error) {
      console.error('Error analyzing:', error);
      alert('Failed to analyze error. Please try again.');
    } finally {
      setAnalyzingId(null);
    }
  };

  if (!isOpen) return null;

  return (
    <div 
      className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-lg shadow-xl w-full max-w-6xl max-h-[90vh] flex flex-col"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Error Monitor</h2>
            <p className="text-sm text-gray-500">Development Only - {allErrors.length} total errors</p>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={togglePolling}
              className={`px-3 py-1 text-sm rounded ${
                isPolling ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-700'
              }`}
            >
              {isPolling ? '● Auto-refresh ON' : '○ Auto-refresh OFF'}
            </button>
            <button
              onClick={fetchBackendErrors}
              className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200"
            >
              Refresh
            </button>
            <button
              onClick={exportErrors}
              className="px-3 py-1 text-sm bg-gray-100 text-gray-700 rounded hover:bg-gray-200"
            >
              Export JSON
            </button>
            <button
              onClick={clearErrors}
              className="px-3 py-1 text-sm bg-red-100 text-red-700 rounded hover:bg-red-200"
            >
              Clear All
            </button>
            <button
              onClick={onClose}
              className="p-2 hover:bg-gray-100 rounded-full transition-colors"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        {/* Tabs */}
        <div className="flex border-b overflow-x-auto">
          {categories.map(category => {
            const count = category === 'All' 
              ? allErrors.length 
              : allErrors.filter(e => e.category === category).length;
            
            return (
              <button
                key={category}
                onClick={() => setActiveTab(category)}
                className={`px-4 py-2 text-sm font-medium whitespace-nowrap ${
                  activeTab === category
                    ? 'border-b-2 border-blue-600 text-blue-600'
                    : 'text-gray-600 hover:text-gray-900'
                }`}
              >
                {category} {count > 0 && `(${count})`}
              </button>
            );
          })}
        </div>

        {/* Search */}
        <div className="p-4 border-b">
          <input
            type="text"
            placeholder="Search errors..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Error List */}
        <div className="flex-1 overflow-y-auto p-4">
          {filteredErrors.length === 0 ? (
            <div className="text-center py-12 text-gray-500">
              <svg className="w-12 h-12 mx-auto mb-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p>No errors found</p>
            </div>
          ) : (
            <div className="space-y-3">
              {filteredErrors.map(error => (
                <div key={error.id} className="border rounded-lg p-4 hover:shadow-md transition-shadow">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <span className={`px-2 py-1 text-xs font-medium rounded ${getCategoryColor(error.category)}`}>
                          {error.category}
                        </span>
                        <span className="text-xs text-gray-500">
                          {new Date(error.timestamp).toLocaleString()}
                        </span>
                        {error.source && (
                          <span className="text-xs text-gray-500">
                            @ {error.source}
                          </span>
                        )}
                      </div>
                      <p className="text-sm font-medium text-gray-900">{error.message}</p>
                      
                      {/* AI Suggestion Display */}
                      {aiSuggestions[error.id] && (
                        <div className="mt-3 p-3 bg-gradient-to-r from-blue-50 to-purple-50 border border-blue-200 rounded-lg">
                          <div className="flex items-start gap-2">
                            <svg className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                            </svg>
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center gap-2 mb-1">
                                <span className="text-xs font-semibold text-blue-800">🤖 AI Suggestion</span>
                                {aiSuggestions[error.id].cached && (
                                  <span className="text-xs text-gray-500">(cached)</span>
                                )}
                              </div>
                              <p className="text-sm text-gray-700 whitespace-pre-wrap break-words">{aiSuggestions[error.id].suggestion}</p>
                            </div>
                          </div>
                        </div>
                      )}

                      {error.metadata && Object.keys(error.metadata).length > 0 && (
                        <div className="mt-2 text-xs text-gray-600">
                          {Object.entries(error.metadata).map(([key, value]) => (
                            <div key={key}>
                              <span className="font-medium">{key}:</span> {String(value)}
                            </div>
                          ))}
                        </div>
                      )}
                      <div className="flex items-center gap-2 mt-3">
                        {error.stackTrace && (
                          <button
                            onClick={() => setExpandedId(expandedId === error.id ? null : error.id)}
                            className="text-xs text-blue-600 hover:text-blue-800"
                          >
                            {expandedId === error.id ? 'Hide' : 'Show'} Stack Trace
                          </button>
                        )}
                        {!aiSuggestions[error.id] && (
                          <button
                            onClick={() => handleAnalyzeError(error)}
                            disabled={analyzingId === error.id}
                            className={`flex items-center gap-1 px-3 py-1 text-xs rounded-full transition-colors ${
                              analyzingId === error.id
                                ? 'bg-gray-100 text-gray-400 cursor-wait'
                                : 'bg-gradient-to-r from-blue-500 to-purple-500 text-white hover:from-blue-600 hover:to-purple-600'
                            }`}
                          >
                            {analyzingId === error.id ? (
                              <>
                                <svg className="animate-spin h-3 w-3" fill="none" viewBox="0 0 24 24">
                                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                </svg>
                                Analyzing...
                              </>
                            ) : (
                              <>
                                🤖 Analyze with AI
                              </>
                            )}
                          </button>
                        )}
                      </div>
                      {expandedId === error.id && error.stackTrace && (
                        <pre className="mt-2 p-2 bg-gray-50 rounded text-xs overflow-x-auto">
                          {error.stackTrace}
                        </pre>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
