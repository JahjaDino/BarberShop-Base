import { useEffect, useState } from 'react'
import barberShopImage from '../../assets/optimized/barberShop1-hero.webp'
import LoginCard from './LoginCard'

function HeroSection() {
  const [highlighted, setHighlighted] = useState(false)
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login')

  const highlightLoginCard = () => {
    setAuthMode('login')
    setHighlighted(true)
    window.setTimeout(() => setHighlighted(false), 2000)
  }

  const focusCard = (mode: 'login' | 'register') => {
    setAuthMode(mode)

    document.getElementById('login-card')?.scrollIntoView({
      behavior: 'smooth',
      block: 'center',
    })

    setHighlighted(true)
    window.setTimeout(() => setHighlighted(false), 2000)
  }

  useEffect(() => {
    window.addEventListener('highlight-login-card', highlightLoginCard)

    return () => {
      window.removeEventListener('highlight-login-card', highlightLoginCard)
    }
  }, [])

  return (
    <section id="home" className="relative overflow-hidden">
      <img
        src={barberShopImage}
        alt="Classic Cuts Barbershop"
        className="absolute inset-0 h-full w-full object-cover"
      />

      <div className="absolute inset-0 bg-gradient-to-r from-black/75 via-black/45 to-black/70" />
      <div className="absolute inset-0 bg-emerald-950/15" />

      <div className="relative mx-auto grid min-h-screen max-w-[1450px] items-center gap-12 px-5 py-10 sm:px-8 lg:grid-cols-[minmax(0,720px)_390px] lg:justify-between lg:px-12 xl:px-16">
        <div className="max-w-[760px]">
          <p className="text-xs font-semibold uppercase tracking-[0.32em] text-amber-200/80">
            Moderni barbershop
          </p>

          <h1 className="mt-5 text-5xl font-black leading-[0.95] text-stone-50 sm:text-6xl lg:text-7xl">
            VIŠE OD FRIZURE.
            <span className="mt-2 block text-amber-100">DOŽIVLJAJ.</span>
          </h1>

          <p className="mt-6 max-w-2xl text-base leading-8 text-stone-200 sm:text-lg">
            Rezerviši svog frizera, prati termine i uštedi vrijeme u elegantnoj
            aplikaciji napravljenoj za moderni barbershop.
          </p>

          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <button
              type="button"
              onClick={() => focusCard('login')}
              className="rounded-2xl bg-amber-200 px-6 py-4 text-center text-sm font-bold uppercase tracking-[0.2em] text-emerald-950 transition-all duration-300 hover:-translate-y-0.5 hover:bg-[#d6b56c] hover:shadow-lg"
            >
              Prijavi se
            </button>

            <button
              type="button"
              onClick={() => focusCard('register')}
              className="rounded-2xl border border-amber-200/35 bg-black/25 px-6 py-4 text-center text-sm font-bold uppercase tracking-[0.2em] text-amber-100 backdrop-blur transition-all duration-300 hover:-translate-y-0.5 hover:border-amber-200/50 hover:bg-amber-100/10"
            >
              Registruj se
            </button>
          </div>

          <div className="mt-8 grid max-w-xl grid-cols-3 gap-4 text-center">
            {[
              ['4.9', 'Ocjena'],
              ['2k+', 'Termina'],
              ['3', 'Frizera'],
            ].map(([value, label]) => (
              <div
                key={label}
                className="rounded-2xl border border-amber-200/15 bg-black/35 p-5 backdrop-blur"
              >
                <p className="text-2xl font-bold text-amber-100">{value}</p>
                <p className="mt-1 text-xs uppercase tracking-[0.18em] text-stone-300">
                  {label}
                </p>
              </div>
            ))}
          </div>
        </div>

        <div className="relative w-full max-w-[390px] justify-self-end">
          <div className="absolute -inset-5 rounded-[2rem] bg-amber-200/10 blur-3xl" />
          <div className="relative">
            <LoginCard mode={authMode} highlighted={highlighted} />
          </div>
        </div>
      </div>
    </section>
  )
}

export default HeroSection
