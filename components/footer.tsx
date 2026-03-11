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
    <footer className="bg-zinc-900 border-t border-zinc-800">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 md:py-16">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 mb-12">
          <div className="sm:col-span-2 lg:col-span-1">
            <Link href="/" className="flex items-center gap-2 mb-4 group">
              <div className="w-8 h-8 bg-yellow-400 rounded-lg flex items-center justify-center font-bold text-zinc-900">PA</div>
              <span className="text-lg font-bold text-yellow-400">Potomac</span>
            </Link>
            <p className="text-zinc-400 text-sm">Advanced financial analysis toolkit built with modern technology and powerful capabilities.</p>
          </div>

          {Object.entries(links).map(([category, items]) => (
            <div key={category}>
              <h4 className="font-semibold text-zinc-100 mb-4 text-sm uppercase tracking-wide">{category}</h4>
              <ul className="space-y-3">
                {items.map((item) => (
                  <li key={item.label}>
                    <a
                      href={item.href}
                      target={item.href.startsWith('http') ? '_blank' : undefined}
                      rel={item.href.startsWith('http') ? 'noopener noreferrer' : undefined}
                      className="text-zinc-400 hover:text-yellow-400 transition-colors text-sm flex items-center gap-1 min-h-[44px] py-2"
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

        <div className="border-t border-zinc-800 mb-8"></div>

        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          <p className="text-zinc-500 text-sm">© {currentYear} PotomacAnalyst. All rights reserved. MIT License.</p>
          <div className="flex items-center gap-4">
            <a
              href="https://github.com/sohaibali73/PotomacAnalystWinUI3"
              target="_blank"
              rel="noopener noreferrer"
              className="p-2 hover:bg-zinc-800 rounded-lg transition-colors text-zinc-400 hover:text-yellow-400 min-w-[44px] min-h-[44px] flex items-center justify-center"
              title="GitHub"
            >
              <Github size={20} />
            </a>
            <a
              href="mailto:support@potomacanalyst.com"
              className="p-2 hover:bg-zinc-800 rounded-lg transition-colors text-zinc-400 hover:text-yellow-400 min-w-[44px] min-h-[44px] flex items-center justify-center"
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
