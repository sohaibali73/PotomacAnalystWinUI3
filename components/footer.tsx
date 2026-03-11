'use client';

import { Github, Mail, ExternalLink } from 'lucide-react';
import Link from 'next/link';

export default function Footer() {
  const currentYear = new Date().getFullYear();

  const links = {
    Documentation: [
      { label: 'Architecture', href: '#architecture' },
      { label: 'Modules', href: '#modules' },
      { label: 'API Reference', href: '#api-reference' },
      { label: 'Integration', href: '#integration' },
    ],
    Resources: [
      { label: 'GitHub Repository', href: 'https://github.com/sohaibali73/PotomacAnalystWinUI3' },
      { label: 'User Guide', href: '#' },
      { label: 'FAQ', href: '#' },
      { label: 'Support', href: '#' },
    ],
    Community: [
      { label: 'GitHub Issues', href: 'https://github.com/sohaibali73/PotomacAnalystWinUI3/issues' },
      { label: 'Discussions', href: 'https://github.com/sohaibali73/PotomacAnalystWinUI3/discussions' },
      { label: 'Contributing', href: '#' },
      { label: 'License', href: '#' },
    ],
  };

  return (
    <footer className="bg-surface-primary border-t border-border-color">
      <div className="container-max py-12 md:py-16">
        {/* Footer Content */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 mb-12">
          {/* Brand */}
          <div className="sm:col-span-2 lg:col-span-1">
            <Link href="/" className="flex items-center gap-2 mb-4 group">
              <div className="w-8 h-8 bg-accent-primary rounded-lg flex items-center justify-center font-bold text-slate-900">
                PA
              </div>
              <span className="text-lg font-bold text-accent-primary">Potomac</span>
            </Link>
            <p className="text-text-secondary text-sm">
              Advanced financial analysis toolkit built with modern technology and powerful capabilities.
            </p>
          </div>

          {/* Links Sections */}
          {Object.entries(links).map(([category, items]) => (
            <div key={category}>
              <h4 className="font-semibold text-text-primary mb-4 text-sm uppercase tracking-wide">
                {category}
              </h4>
              <ul className="space-y-3">
                {items.map((item) => (
                  <li key={item.label}>
                    <a
                      href={item.href}
                      target={item.href.startsWith('http') ? '_blank' : undefined}
                      rel={item.href.startsWith('http') ? 'noopener noreferrer' : undefined}
                      className="text-text-secondary hover:text-accent-primary transition-colors text-sm flex items-center gap-1"
                    >
                      {item.label}
                      {item.href.startsWith('http') && <ExternalLink size={12} />}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Divider */}
        <div className="border-t border-border-color mb-8"></div>

        {/* Bottom Section */}
        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          {/* Copyright */}
          <p className="text-text-muted text-sm">
            © {currentYear} PotomacAnalyst. All rights reserved. MIT License.
          </p>

          {/* Social Links */}
          <div className="flex items-center gap-4">
            <a
              href="https://github.com/sohaibali73/PotomacAnalystWinUI3"
              target="_blank"
              rel="noopener noreferrer"
              className="p-2 hover:bg-surface-secondary rounded-lg transition-colors text-text-secondary hover:text-accent-primary"
              title="GitHub"
            >
              <Github size={20} />
            </a>
            <a
              href="mailto:support@potomacanalyst.com"
              className="p-2 hover:bg-surface-secondary rounded-lg transition-colors text-text-secondary hover:text-accent-primary"
              title="Email Support"
            >
              <Mail size={20} />
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
}
