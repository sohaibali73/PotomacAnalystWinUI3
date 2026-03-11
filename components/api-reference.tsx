'use client';

import { ChevronDown } from 'lucide-react';
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
    category: 'Analysis & Backtest',
    items: [
      { method: 'POST', path: '/api/backtest/run', description: 'Run a backtest on a strategy with historical data' },
      { method: 'GET', path: '/api/backtest/results/:id', description: 'Get backtest results and performance metrics' },
      { method: 'POST', path: '/api/afl/validate', description: 'Validate AFL code syntax and structure' },
    ],
  },
  {
    category: 'Data & Research',
    items: [
      { method: 'GET', path: '/api/market/quotes/:symbol', description: 'Get current market data for a symbol' },
      { method: 'GET', path: '/api/research/content/:id', description: 'Retrieve knowledge base content by ID' },
      { method: 'POST', path: '/api/research/search', description: 'Search across knowledge base and external sources' },
    ],
  },
  {
    category: 'Skills & Extensions',
    items: [
      { method: 'GET', path: '/api/skills/list', description: 'List all available skills and custom extensions' },
      { method: 'POST', path: '/api/skills/create', description: 'Create a new custom skill or tool' },
      { method: 'POST', path: '/api/skills/:id/execute', description: 'Execute a specific skill with parameters' },
    ],
  },
];

const MethodBadge = ({ method }: { method: string }) => {
  const colors: Record<string, string> = {
    GET: 'bg-blue-500/20 text-blue-400',
    POST: 'bg-green-500/20 text-green-400',
    PUT: 'bg-yellow-500/20 text-yellow-400',
    DELETE: 'bg-red-500/20 text-red-400',
  };
  const color = colors[method] || colors.GET;
  return <span className={`px-2 py-1 rounded text-xs font-mono font-semibold ${color}`}>{method}</span>;
};

function CategorySection({ category, items }: { category: string; items: typeof endpoints[0]['items'] }) {
  const [isOpen, setIsOpen] = useState(true);

  return (
    <div className="mb-4">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-full flex items-center justify-between p-4 bg-zinc-800 hover:bg-zinc-700 rounded-lg border border-zinc-700 transition-colors text-left min-h-[44px]"
      >
        <h4 className="font-semibold text-zinc-100">{category}</h4>
        <ChevronDown size={20} className={`text-zinc-400 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
      </button>

      {isOpen && (
        <div className="mt-3 space-y-3">
          {items.map((item, idx) => (
            <div key={idx} className="ml-4 p-4 bg-zinc-950 rounded-lg border border-zinc-800">
              <div className="flex items-center gap-3 mb-2">
                <MethodBadge method={item.method} />
                <code className="text-xs sm:text-sm font-mono text-zinc-400 flex-1 overflow-x-auto">{item.path}</code>
              </div>
              <p className="text-sm text-zinc-400">{item.description}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default function ApiReference() {
  return (
    <section id="api-reference" className="py-20 md:py-32 bg-zinc-950">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="max-w-3xl mx-auto text-center mb-16">
          <h2 className="text-4xl font-bold text-zinc-100 mb-4">API Reference</h2>
          <p className="text-xl text-zinc-400">RESTful API endpoints for integrating with PotomacAnalyst services.</p>
        </div>

        <div className="max-w-4xl mx-auto">
          {endpoints.map((section) => (
            <CategorySection key={section.category} category={section.category} items={section.items} />
          ))}

          <div className="mt-8 p-6 bg-yellow-400/10 border border-yellow-400/30 rounded-lg">
            <h4 className="font-semibold text-zinc-100 mb-3">Authentication</h4>
            <p className="text-zinc-400 text-sm mb-4">All API requests require authentication. Include your API key in the Authorization header:</p>
            <code className="block bg-zinc-950 rounded-lg p-4 text-sm font-mono text-amber-100 overflow-x-auto">Authorization: Bearer YOUR_API_KEY</code>
          </div>
        </div>
      </div>
    </section>
  );
}
