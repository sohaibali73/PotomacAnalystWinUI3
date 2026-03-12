'use client';

import { useState } from 'react';

const navItems = [
  { label: 'Overview', href: '#overview' },
  { label: 'Getting Started', href: '#getting-started' },
  { label: 'Modules', href: '#modules' },
  { label: 'Architecture', href: '#architecture' },
  { label: 'API', href: '#api' },
];

export default function Header() {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <header className="fixed top-0 left-0 right-0 z-50 bg-[rgba(251,251,253,0.8)] backdrop-blur-xl border-b border-[#d2d2d7]/50">
      <nav className="max-w-[980px] mx-auto px-6 h-12 flex items-center justify-between">
        <a href="#" className="text-[21px] font-semibold text-[#1d1d1f] tracking-[-0.01em]">
          PotomacAnalyst
        </a>

        <div className="hidden md:flex items-center gap-7">
          {navItems.map((item) => (
            <a
              key={item.label}
              href={item.href}
              className="text-[12px] text-[#1d1d1f]/90 hover:text-[#0071e3] transition-colors"
            >
              {item.label}
            </a>
          ))}
        </div>

        <a
          href="https://github.com/sohaibali73/PotomacAnalystWinUI3"
          target="_blank"
          rel="noopener noreferrer"
          className="hidden md:flex items-center gap-1.5 text-[12px] text-[#0071e3] hover:underline"
        >
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
            <path fillRule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" clipRule="evenodd" />
          </svg>
          View on GitHub
        </a>

        <button
          onClick={() => setIsOpen(!isOpen)}
          className="md:hidden p-2 -mr-2 min-w-[44px] min-h-[44px] flex items-center justify-center"
          aria-label="Toggle menu"
        >
          <svg className="w-5 h-5 text-[#1d1d1f]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            {isOpen ? (
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M6 18L18 6M6 6l12 12" />
            ) : (
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 6h16M4 12h16M4 18h16" />
            )}
          </svg>
        </button>
      </nav>

      {isOpen && (
        <div className="md:hidden bg-[rgba(251,251,253,0.98)] backdrop-blur-xl border-b border-[#d2d2d7]/50">
          <div className="max-w-[980px] mx-auto px-6 py-3 flex flex-col">
            {navItems.map((item) => (
              <a
                key={item.label}
                href={item.href}
                onClick={() => setIsOpen(false)}
                className="text-[14px] text-[#1d1d1f] py-3 border-b border-[#d2d2d7]/30 last:border-0"
              >
                {item.label}
              </a>
            ))}
          </div>
        </div>
      )}
    </header>
  );
}
