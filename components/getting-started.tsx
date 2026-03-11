'use client';

import { CheckCircle2, Download, Cog, BookOpen } from 'lucide-react';

const steps = [
  {
    icon: Download,
    title: 'Installation',
    description: 'Install via Microsoft Store or download the latest release package from GitHub.',
    details: [
      '.NET 8 Runtime required',
      'Windows 10 version 1809 or later',
      '2 GB RAM minimum (4 GB recommended)',
      '500 MB storage space',
    ],
  },
  {
    icon: CheckCircle2,
    title: 'Setup & Configuration',
    description: 'Configure initial settings, connect data sources, and customize your workspace.',
    details: [
      'Create user account',
      'Configure data sources',
      'Set up preferences',
      'Enable integrations',
    ],
  },
  {
    icon: Cog,
    title: 'Development Setup',
    description: 'For developers: clone the repository, open in Visual Studio 2022, and build the solution.',
    details: [
      'Clone from GitHub',
      'Open PotomacAnalyst.sln',
      'Build solution (Debug/Release)',
      'Run from Visual Studio',
    ],
  },
  {
    icon: BookOpen,
    title: 'Explore Documentation',
    description: 'Review detailed guides on features, architecture, and API integration patterns.',
    details: [
      'Feature documentation',
      'Architecture guides',
      'API reference',
      'Code examples',
    ],
  },
];

const requirements = [
  { category: 'Minimum Requirements', items: ['Windows 10 v1809+', '1 GHz processor', '2 GB RAM', '500 MB storage'] },
  {
    category: 'Recommended Requirements',
    items: ['Windows 11 (22H2+)', 'Multi-core processor 2 GHz+', '8 GB+ RAM', '1 GB SSD storage'],
  },
  { category: 'Developer Requirements', items: ['Visual Studio 2022', '.NET 8 SDK', 'Windows App SDK 1.8', 'Git'] },
];

export default function GettingStarted() {
  return (
    <section id="getting-started" className="py-20 md:py-32 bg-background">
      <div className="container-max">
        {/* Section Header */}
        <div className="max-w-3xl mx-auto text-center mb-16">
          <h2 className="section-heading">Getting Started</h2>
          <p className="text-xl text-text-secondary">
            Quick start guide to install, configure, and begin using PotomacAnalyst.
          </p>
        </div>

        {/* Steps */}
        <div className="max-w-4xl mx-auto mb-20">
          <div className="space-y-8">
            {steps.map((step, index) => {
              const IconComponent = step.icon;
              return (
                <div key={step.title} className="flex gap-6">
                  {/* Step Number */}
                  <div className="flex-shrink-0 flex items-center">
                    <div className="flex items-center justify-center w-12 h-12 rounded-full bg-accent-primary text-slate-900 font-bold text-lg">
                      {index + 1}
                    </div>
                  </div>

                  {/* Content */}
                  <div className="flex-1 pt-2">
                    <div className="flex items-center gap-3 mb-3">
                      <IconComponent size={24} className="text-accent-primary flex-shrink-0" />
                      <h3 className="text-2xl font-bold text-text-primary">{step.title}</h3>
                    </div>
                    <p className="text-text-secondary mb-4">{step.description}</p>

                    {/* Details */}
                    <ul className="space-y-2">
                      {step.details.map((detail) => (
                        <li key={detail} className="flex items-center gap-2 text-text-secondary">
                          <span className="w-1.5 h-1.5 bg-accent-primary rounded-full"></span>
                          {detail}
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* System Requirements */}
        <div className="max-w-4xl mx-auto">
          <h3 className="subsection-heading mb-8">System Requirements</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {requirements.map((req) => (
              <div key={req.category} className="card">
                <h4 className="font-semibold text-text-primary mb-4">{req.category}</h4>
                <ul className="space-y-3">
                  {req.items.map((item) => (
                    <li key={item} className="flex items-center gap-3 text-sm text-text-secondary">
                      <span className="w-1.5 h-1.5 bg-accent-primary rounded-full flex-shrink-0"></span>
                      {item}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>

        {/* Installation Methods */}
        <div className="mt-16 max-w-4xl mx-auto">
          <h3 className="subsection-heading mb-8">Installation Methods</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[
              {
                title: 'Microsoft Store',
                description: 'Recommended method - easiest installation and automatic updates',
                badge: 'Recommended',
              },
              {
                title: 'Direct Download',
                description: 'Download MSIX package from releases and install manually',
                badge: 'Manual',
              },
              {
                title: 'Development Build',
                description: 'Clone repository and build directly from Visual Studio',
                badge: 'Advanced',
              },
            ].map((method) => (
              <div key={method.title} className="bg-surface-primary rounded-lg p-6 border border-border-color">
                <div className="flex items-center justify-between mb-3">
                  <h4 className="font-semibold text-text-primary">{method.title}</h4>
                  <span className="text-xs px-2 py-1 bg-accent-primary/20 text-accent-primary rounded font-mono">
                    {method.badge}
                  </span>
                </div>
                <p className="text-text-secondary text-sm">{method.description}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
