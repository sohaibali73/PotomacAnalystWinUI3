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
    <header className="sticky top-0 z-50 bg-surface-primary/95 backdrop-blur border-b border-border-color">
      <div className="container-max">
        <nav className="flex items-center justify-between h-16 md:h-20">
          {/* Logo */}
          <Link href="/" className="flex items-center gap-2 group">
            <div className="w-8 h-8 bg-accent-primary rounded-lg flex items-center justify-center font-bold text-slate-900 group-hover:scale-110 transition-transform">
              PA
            </div>
            <span className="text-xl font-bold text-accent-primary hidden sm:inline">Potomac</span>
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center gap-8">
            {navLinks.map((link) => (
              <a
                key={link.href}
                href={link.href}
                className="text-text-secondary hover:text-accent-primary transition-colors text-sm font-medium"
              >
                {link.label}
              </a>
            ))}
          </div>

          {/* Right Actions */}
          <div className="flex items-center gap-4">
            <a
              href="https://github.com/sohaibali73/PotomacAnalystWinUI3"
              target="_blank"
              rel="noopener noreferrer"
              className="btn-secondary text-xs sm:text-sm"
            >
              GitHub
            </a>

            {/* Mobile Menu Button */}
            <button
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="md:hidden p-2 hover:bg-surface-secondary rounded-lg transition-colors"
            >
              {mobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
            </button>
          </div>
        </nav>

        {/* Mobile Menu */}
        {mobileMenuOpen && (
          <div className="md:hidden pb-4 space-y-2 border-t border-border-color pt-4">
            {navLinks.map((link) => (
              <a
                key={link.href}
                href={link.href}
                className="block px-4 py-2 text-text-secondary hover:text-accent-primary hover:bg-surface-secondary rounded-lg transition-colors"
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
