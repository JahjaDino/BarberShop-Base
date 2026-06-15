import AboutSection from '../features/landing/AboutSection'
import ContactSection from '../features/landing/ContactSection'
import GallerySection from '../features/landing/GallerySection'
import HeroSection from '../features/landing/HeroSection'
import Sidebar from '../components/layout/Sidebar'


function HomePage() {
  return (
    <main className="min-h-screen bg-[#06110d] text-stone-100">
      <div className="flex min-h-screen flex-col lg:flex-row">
        <Sidebar />
        <div className="relative flex-1 overflow-hidden">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_10%,rgba(245,213,145,0.16),transparent_28%),radial-gradient(circle_at_78%_22%,rgba(34,197,94,0.16),transparent_26%),linear-gradient(135deg,#07110d_0%,#0b1711_45%,#030504_100%)]" />
          <div className="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0.035)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,0.035)_1px,transparent_1px)] bg-[size:64px_64px] opacity-25" />
          <div className="relative">
            <HeroSection />
            <AboutSection />
            <GallerySection />
            <ContactSection />
          </div>
        </div>
      </div>
    </main>
  )
}

export default HomePage
