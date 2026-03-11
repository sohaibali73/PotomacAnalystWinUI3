'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Menu, X } from 'lucide-react';

export default function Header() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const navLinks = [
    { href: '#getting-started', label: 'Getting Started' },
    { href: '#architecture', label: 'Architecture' },
    { href: '#modules', label: 'Modules' },
    { href: '#api-reference', label: 'API Reference' },
    { href: '#integration', label: 'Integration' },
  ];

  return (
    <header className="sticky top-0 z-50 bg-zinc-900/95 backdrop-blur border-b border-zinc-800">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <nav className="flex items-center justify-between h-16 md:h-20">
          <Link href="/" className="flex items-center gap-2 group">
            <div className="w-8 h-8 bg-yellow-400 rounded-lg flex items-center justify-center font-bold text-zinc-900 group-hover:scale-110 transition-transform">
              PA
            </div>
            <span className="text-xl font-bold text-yellow-400 hidden sm:inline">Potomac</span>
          </Link>

          <div className="hidden md:flex items-center gap-8">
            {navLinks.map((link) => (
              <a
                key={link.href}
                href={link.href}
                className="text-zinc-400 hover:text-yellow-400 transition-colors text-sm font-medium"
              >
                {link.label}
              </a>
            ))}
          </div>

          <div className="flex items-center gap-4">
            <a
              href="https://github.com/sohaibali73/PotomacAnalystWinUI3"
              target="_blank"
              rel="noopener noreferrer"
              className="px-4 py-2 bg-zinc-800 text-zinc-100 text-xs sm:text-sm font-medium rounded-lg border border-zinc-700 hover:bg-zinc-700 transition-colors"
            >
              GitHub
            </a>

            <button
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="md:hidden p-2 hover:bg-zinc-800 rounded-lg transition-colors min-w-[44px] min-h-[44px] flex items-center justify-center"
            >
              {mobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
            </button>
          </div>
        </nav>

        {mobileMenuOpen && (
          <div className="md:hidden pb-4 space-y-2 border-t border-zinc-800 pt-4">
            {navLinks.map((link) => (
              <a
                key={link.href}
                href={link.href}
                className="block px-4 py-3 text-zinc-400 hover:text-yellow-400 hover:bg-zinc-800 rounded-lg transition-colors min-h-[44px]"
                onClick={() => setMobileMenuOpen(false)}
              >
                {link.label}
              </a>
            ))}
          </div>
        )}
      </div>
    </header>
  );
}
