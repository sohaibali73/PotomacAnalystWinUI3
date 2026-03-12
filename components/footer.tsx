const links = {
  Documentation: [
    { label: 'Overview', href: '#overview' },
    { label: 'Getting Started', href: '#getting-started' },
    { label: 'Modules', href: '#modules' },
    { label: 'Architecture', href: '#architecture' },
    { label: 'API Reference', href: '#api' },
  ],
  Resources: [
    { label: 'GitHub Repository', href: 'https://github.com/sohaibali73/PotomacAnalystWinUI3', external: true },
    { label: 'User Guide', href: '#' },
    { label: 'FAQ', href: '#' },
    { label: 'Support', href: '#' },
  ],
  Community: [
    { label: 'GitHub Issues', href: 'https://github.com/sohaibali73/PotomacAnalystWinUI3/issues', external: true },
    { label: 'Discussions', href: 'https://github.com/sohaibali73/PotomacAnalystWinUI3/discussions', external: true },
    { label: 'Contributing', href: '#' },
    { label: 'License', href: '#' },
  ],
};

export default function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-[#f5f5f7] border-t border-[#d2d2d7]/50">
      <div className="max-w-[980px] mx-auto px-6 py-12 md:py-16">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 mb-10">
          <div className="sm:col-span-2 lg:col-span-1">
            <a href="#" className="text-[21px] font-semibold text-[#1d1d1f] mb-4 block">
              PotomacAnalyst
            </a>
            <p className="text-[12px] text-[#86868b] leading-[1.5] max-w-[200px]">
              Advanced financial analysis toolkit built with modern technology.
            </p>
          </div>

          {Object.entries(links).map(([category, items]) => (
            <div key={category}>
              <h4 className="text-[12px] font-semibold text-[#1d1d1f] mb-4">{category}</h4>
              <ul className="space-y-2">
                {items.map((item) => (
                  <li key={item.label}>
                    <a
                      href={item.href}
                      target={item.external ? '_blank' : undefined}
                      rel={item.external ? 'noopener noreferrer' : undefined}
                      className="text-[12px] text-[#424245] hover:text-[#1d1d1f] hover:underline transition-colors inline-flex items-center gap-1 py-1 min-h-[44px]"
                    >
                      {item.label}
                      {item.external && (
                        <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                        </svg>
                      )}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="border-t border-[#d2d2d7]/50 pt-6">
          <div className="flex flex-col md:flex-row items-center justify-between gap-4">
            <p className="text-[12px] text-[#86868b]">
              Copyright © {currentYear} PotomacAnalyst. All rights reserved.
            </p>
            <div className="flex items-center gap-6">
              <a href="#" className="text-[12px] text-[#424245] hover:text-[#1d1d1f] hover:underline">
                Privacy Policy
              </a>
              <a href="#" className="text-[12px] text-[#424245] hover:text-[#1d1d1f] hover:underline">
                Terms of Use
              </a>
              <a href="#" className="text-[12px] text-[#424245] hover:text-[#1d1d1f] hover:underline">
                Legal
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
