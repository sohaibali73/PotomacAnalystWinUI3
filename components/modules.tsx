'use client';

import { BarChart3, MessageSquare, Book, TrendingUp, Code, Sparkles, Zap, Users, Settings, Layers, Search, Cpu } from 'lucide-react';

const modules = [
  {
    icon: BarChart3,
    name: 'Dashboard',
    description: 'Central hub with real-time analytics, performance metrics, and customizable widgets for instant insights.',
  },
  {
    icon: Code,
    name: 'AFL Generator',
    description: 'Advanced Formula Language code generation with drag-and-drop interface, syntax highlighting, and version control.',
  },
  {
    icon: MessageSquare,
    name: 'Chat',
    description: 'AI-powered conversational interface with natural language processing, context awareness, and file analysis.',
  },
  {
    icon: Book,
    name: 'Knowledge Base',
    description: 'Organized repository for research materials, documentation, and collaborative knowledge management.',
  },
  {
    icon: TrendingUp,
    name: 'Backtest',
    description: 'Historical data testing and strategy validation with comprehensive performance metrics and risk assessment.',
  },
  {
    icon: Sparkles,
    name: 'Reverse Engineer',
    description: 'Code analysis and deconstruction tools for understanding, documenting, and optimizing systems.',
  },
  {
    icon: Users,
    name: 'Content',
    description: 'Document management with rich text editing, templates, version control, and collaborative features.',
  },
  {
    icon: Zap,
    name: 'Deck Generator',
    description: 'Professional presentation and report generation with data integration and design customization.',
  },
  {
    icon: Cpu,
    name: 'Autopilot',
    description: 'Automated analysis workflows and batch processing with scheduling and error handling.',
  },
  {
    icon: Layers,
    name: 'Skills',
    description: 'Custom tool and skill management system for extending functionality and building reusable components.',
  },
  {
    icon: Search,
    name: 'Researcher',
    description: 'Advanced research capabilities with database integration, API support, and automated data gathering.',
  },
  {
    icon: Settings,
    name: 'Developer',
    description: 'Development tools, debugging utilities, and API testing for custom module development.',
  },
];

export default function Modules() {
  return (
    <section id="modules" className="py-20 md:py-32 bg-background">
      <div className="container-max">
        {/* Section Header */}
        <div className="max-w-3xl mx-auto text-center mb-16">
          <h2 className="section-heading">Core Modules</h2>
          <p className="text-xl text-text-secondary">
            Thirteen powerful modules designed to cover every aspect of financial analysis and research.
          </p>
        </div>

        {/* Modules Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {modules.map((module) => {
            const IconComponent = module.icon;
            return (
              <div
                key={module.name}
                className="card group hover:border-accent-primary/50 hover:bg-surface-secondary transition-all duration-300"
              >
                <div className="flex items-start gap-4">
                  <div className="p-3 bg-surface-secondary rounded-lg group-hover:bg-accent-primary/20 transition-colors">
                    <IconComponent size={24} className="text-accent-primary" />
                  </div>
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-text-primary mb-2">{module.name}</h3>
                    <p className="text-text-secondary text-sm leading-relaxed">{module.description}</p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}
