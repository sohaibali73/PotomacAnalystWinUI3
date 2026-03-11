'use client';

import { ArrowRight, BookOpen, Zap } from 'lucide-react';
import Link from 'next/link';

export default function Hero() {
  return (
    <section className="relative overflow-hidden bg-gradient-to-b from-surface-primary via-background to-background pt-20 md:pt-32 pb-16 md:pb-24">
      {/* Background accent */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-0 right-0 w-96 h-96 bg-accent-secondary/5 rounded-full blur-3xl"></div>
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-accent-primary/5 rounded-full blur-3xl"></div>
      </div>

      <div className="container-max relative z-10">
        <div className="max-w-3xl mx-auto text-center">
          {/* Badge */}
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-surface-secondary rounded-full border border-border-color mb-6">
            <Zap size={16} className="text-accent-primary" />
            <span className="text-sm text-text-secondary">Complete API Documentation</span>
          </div>

          {/* Main Heading */}
          <h1 className="text-4xl md:text-6xl font-bold text-text-primary mb-6 leading-tight">
            Build with{' '}
            <span className="bg-gradient-to-r from-accent-primary via-amber-300 to-yellow-200 bg-clip-text text-transparent">
              PotomacAnalyst
            </span>
          </h1>

          {/* Subheading */}
          <p className="text-lg md:text-xl text-text-secondary mb-8 max-w-2xl mx-auto">
            Advanced financial analysis toolkit built on modern architecture. Explore comprehensive documentation, integration guides, and API references to build powerful analytical applications.
          </p>

          {/* CTA Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center mb-16">
            <a href="#getting-started" className="btn-primary inline-flex items-center justify-center gap-2">
              Get Started
              <ArrowRight size={18} />
            </a>
            <a href="#modules" className="btn-secondary inline-flex items-center justify-center gap-2">
              <BookOpen size={18} />
              Explore Modules
            </a>
          </div>

          {/* Feature Highlights */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-12 border-t border-border-color">
            <div className="text-center">
              <div className="text-3xl font-bold text-accent-primary mb-2">13+</div>
              <p className="text-text-secondary">Core Modules</p>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-accent-primary mb-2">.NET 8</div>
              <p className="text-text-secondary">Modern Stack</p>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-accent-primary mb-2">100%</div>
              <p className="text-text-secondary">WinUI 3</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
