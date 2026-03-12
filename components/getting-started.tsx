export default function GettingStarted() {
  return (
    <section id="getting-started" className="py-20 md:py-28 bg-[#f5f5f7]">
      <div className="max-w-[980px] mx-auto px-6">
        <div className="text-center mb-16">
          <h2 className="text-[32px] md:text-[48px] font-semibold text-[#1d1d1f] leading-[1.08] tracking-[-0.003em] mb-4">
            Get started in minutes.
          </h2>
          <p className="text-[19px] md:text-[21px] text-[#86868b] max-w-[600px] mx-auto">
            Set up PotomacAnalyst quickly with our streamlined installation process.
          </p>
        </div>

        <div className="grid md:grid-cols-2 gap-6">
          <div className="bg-white rounded-2xl p-8 md:p-10 shadow-sm">
            <div className="w-12 h-12 bg-[#f5f5f7] rounded-full flex items-center justify-center mb-6">
              <svg className="w-6 h-6 text-[#1d1d1f]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
              </svg>
            </div>
            <h3 className="text-[24px] font-semibold text-[#1d1d1f] mb-4">System Requirements</h3>
            <ul className="space-y-3 text-[17px] text-[#1d1d1f]">
              <li className="flex items-start gap-3">
                <svg className="w-5 h-5 text-[#34c759] mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                </svg>
                <span>Windows 10 version 1809 or later</span>
              </li>
              <li className="flex items-start gap-3">
                <svg className="w-5 h-5 text-[#34c759] mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                </svg>
                <span>.NET 8.0 Runtime</span>
              </li>
              <li className="flex items-start gap-3">
                <svg className="w-5 h-5 text-[#34c759] mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                </svg>
                <span>Windows App SDK 1.4+</span>
              </li>
              <li className="flex items-start gap-3">
                <svg className="w-5 h-5 text-[#34c759] mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                </svg>
                <span>4GB RAM minimum</span>
              </li>
            </ul>
          </div>

          <div className="bg-white rounded-2xl p-8 md:p-10 shadow-sm">
            <div className="w-12 h-12 bg-[#f5f5f7] rounded-full flex items-center justify-center mb-6">
              <svg className="w-6 h-6 text-[#1d1d1f]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
              </svg>
            </div>
            <h3 className="text-[24px] font-semibold text-[#1d1d1f] mb-4">Installation</h3>
            <div className="space-y-4">
              <div className="flex gap-4">
                <span className="w-7 h-7 bg-[#0071e3] text-white text-[14px] font-medium rounded-full flex items-center justify-center flex-shrink-0">1</span>
                <p className="text-[17px] text-[#1d1d1f]">Clone the repository from GitHub</p>
              </div>
              <div className="flex gap-4">
                <span className="w-7 h-7 bg-[#0071e3] text-white text-[14px] font-medium rounded-full flex items-center justify-center flex-shrink-0">2</span>
                <p className="text-[17px] text-[#1d1d1f]">Open in Visual Studio 2022</p>
              </div>
              <div className="flex gap-4">
                <span className="w-7 h-7 bg-[#0071e3] text-white text-[14px] font-medium rounded-full flex items-center justify-center flex-shrink-0">3</span>
                <p className="text-[17px] text-[#1d1d1f]">Restore NuGet packages</p>
              </div>
              <div className="flex gap-4">
                <span className="w-7 h-7 bg-[#0071e3] text-white text-[14px] font-medium rounded-full flex items-center justify-center flex-shrink-0">4</span>
                <p className="text-[17px] text-[#1d1d1f]">Build and run the application</p>
              </div>
            </div>
          </div>
        </div>

        <div className="mt-8 bg-[#1d1d1f] rounded-2xl p-6 md:p-8 overflow-x-auto">
          <pre className="text-[14px] md:text-[15px] font-mono text-[#f5f5f7] leading-relaxed">
            <code>{`# Clone the repository
git clone https://github.com/sohaibali73/PotomacAnalystWinUI3.git

# Navigate to the project
cd PotomacAnalystWinUI3

# Open in Visual Studio
start PotomacAnalyst.sln`}</code>
          </pre>
        </div>
      </div>
    </section>
  );
}
