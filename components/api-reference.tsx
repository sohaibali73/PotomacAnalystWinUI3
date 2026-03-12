'use client';

import { useState } from 'react';

const endpoints = [
  {
    category: 'Authentication',
    items: [
      { method: 'POST', path: '/api/auth/login', description: 'Authenticate user with credentials' },
      { method: 'POST', path: '/api/auth/logout', description: 'End user session and clear tokens' },
      { method: 'GET', path: '/api/auth/profile', description: 'Get current authenticated user profile' },
    ],
  },
  {
    category: 'Analysis',
    items: [
      { method: 'POST', path: '/api/backtest/run', description: 'Run a backtest on a strategy' },
      { method: 'GET', path: '/api/backtest/results/:id', description: 'Get backtest results and metrics' },
      { method: 'POST', path: '/api/afl/validate', description: 'Validate AFL code syntax' },
    ],
  },
  {
    category: 'Data',
    items: [
      { method: 'GET', path: '/api/market/quotes/:symbol', description: 'Get current market data' },
      { method: 'GET', path: '/api/research/content/:id', description: 'Retrieve knowledge base content' },
      { method: 'POST', path: '/api/research/search', description: 'Search across knowledge base' },
    ],
  },
  {
    category: 'Skills',
    items: [
      { method: 'GET', path: '/api/skills/list', description: 'List all available skills' },
      { method: 'POST', path: '/api/skills/create', description: 'Create a new custom skill' },
      { method: 'POST', path: '/api/skills/:id/execute', description: 'Execute a specific skill' },
    ],
  },
];

const methodStyles: Record<string, string> = {
  GET: 'bg-[#34c759]/10 text-[#248a3d]',
  POST: 'bg-[#0071e3]/10 text-[#0071e3]',
  PUT: 'bg-[#ff9f0a]/10 text-[#c93400]',
  DELETE: 'bg-[#ff3b30]/10 text-[#ff3b30]',
};

export default function ApiReference() {
  const [openCategory, setOpenCategory] = useState<string | null>('Authentication');

  return (
    <section id="api" className="py-20 md:py-28">
      <div className="max-w-[980px] mx-auto px-6">
        <div className="text-center mb-16">
          <h2 className="text-[32px] md:text-[48px] font-semibold text-[#1d1d1f] leading-[1.08] tracking-[-0.003em] mb-4">
            API Reference.
          </h2>
          <p className="text-[19px] md:text-[21px] text-[#86868b] max-w-[600px] mx-auto">
            RESTful endpoints for integrating with PotomacAnalyst services.
          </p>
        </div>

        <div className="space-y-3 mb-12">
          {endpoints.map((section) => (
            <div key={section.category} className="border border-[#d2d2d7]/60 rounded-2xl overflow-hidden">
              <button
                onClick={() => setOpenCategory(openCategory === section.category ? null : section.category)}
                className="w-full flex items-center justify-between p-5 bg-white hover:bg-[#f5f5f7] transition-colors text-left min-h-[56px]"
              >
                <span className="text-[17px] font-semibold text-[#1d1d1f]">{section.category}</span>
                <svg 
                  className={`w-5 h-5 text-[#86868b] transition-transform duration-200 ${openCategory === section.category ? 'rotate-180' : ''}`} 
                  fill="none" 
                  stroke="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>

              {openCategory === section.category && (
                <div className="border-t border-[#d2d2d7]/60 bg-[#f5f5f7]/50">
                  {section.items.map((item, idx) => (
                    <div 
                      key={idx} 
                      className="p-5 border-b border-[#d2d2d7]/40 last:border-b-0"
                    >
                      <div className="flex items-center gap-3 mb-2">
                        <span className={`px-2.5 py-1 rounded-md text-[12px] font-semibold font-mono ${methodStyles[item.method]}`}>
                          {item.method}
                        </span>
                        <code className="text-[14px] font-mono text-[#1d1d1f]">{item.path}</code>
                      </div>
                      <p className="text-[14px] text-[#86868b] ml-[70px]">{item.description}</p>
                    </div>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>

        <div className="bg-[#1d1d1f] rounded-2xl p-6 md:p-8">
          <h4 className="text-[17px] font-semibold text-white mb-4">Authentication</h4>
          <p className="text-[14px] text-[#86868b] mb-4">
            All API requests require authentication. Include your API key in the Authorization header:
          </p>
          <code className="block bg-[#2d2d2d] rounded-xl p-4 text-[14px] font-mono text-[#f5f5f7]">
            Authorization: Bearer YOUR_API_KEY
          </code>
        </div>
      </div>
    </section>
  );
}
