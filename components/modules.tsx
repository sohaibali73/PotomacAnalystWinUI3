const modules = [
  { name: 'Dashboard', description: 'Central hub with real-time analytics and customizable widgets.' },
  { name: 'AFL Generator', description: 'Advanced Formula Language code generation with syntax highlighting.' },
  { name: 'Chat', description: 'AI-powered conversational interface with natural language processing.' },
  { name: 'Knowledge Base', description: 'Organized repository for research materials and documentation.' },
  { name: 'Backtest', description: 'Historical data testing and strategy validation with performance metrics.' },
  { name: 'Reverse Engineer', description: 'Code analysis and deconstruction tools for understanding systems.' },
  { name: 'Content', description: 'Document management with rich text editing and version control.' },
  { name: 'Deck Generator', description: 'Professional presentation and report generation tools.' },
  { name: 'Autopilot', description: 'Automated analysis workflows and batch processing.' },
  { name: 'Skills', description: 'Custom tool and skill management system for extending functionality.' },
  { name: 'Researcher', description: 'Advanced research capabilities with database integration.' },
  { name: 'Developer', description: 'Development tools, debugging utilities, and API testing.' },
];

export default function Modules() {
  return (
    <section id="modules" className="py-20 md:py-28">
      <div className="max-w-[980px] mx-auto px-6">
        <div className="text-center mb-16">
          <h2 className="text-[32px] md:text-[48px] font-semibold text-[#1d1d1f] leading-[1.08] tracking-[-0.003em] mb-4">
            Powerful modules.
          </h2>
          <p className="text-[19px] md:text-[21px] text-[#86868b] max-w-[600px] mx-auto">
            Twelve integrated modules designed for every aspect of financial analysis.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {modules.map((module) => (
            <div
              key={module.name}
              className="group p-6 bg-white rounded-2xl border border-[#d2d2d7]/60 hover:border-[#d2d2d7] hover:shadow-lg transition-all duration-300"
            >
              <h3 className="text-[19px] font-semibold text-[#1d1d1f] mb-2 group-hover:text-[#0071e3] transition-colors">
                {module.name}
              </h3>
              <p className="text-[14px] text-[#86868b] leading-[1.43]">
                {module.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
