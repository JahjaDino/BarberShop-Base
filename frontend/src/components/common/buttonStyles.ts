const baseButton =
  'inline-flex min-w-0 w-full items-center justify-center rounded-2xl px-5 py-3 text-center text-sm font-bold uppercase leading-snug tracking-[0.14em] transition-all duration-300 disabled:cursor-not-allowed disabled:opacity-45 sm:w-auto sm:tracking-[0.16em]'

export const buttonStyles = {
  primary: `${baseButton} border border-amber-200/35 bg-[linear-gradient(135deg,rgba(120,84,28,0.92),rgba(214,181,108,0.72))] text-amber-50 shadow-[0_0_26px_rgba(214,181,108,0.10)] hover:-translate-y-0.5 hover:border-amber-200/50 hover:bg-[linear-gradient(135deg,rgba(145,101,34,0.95),rgba(214,181,108,0.82))] disabled:hover:translate-y-0`,
  secondary: `${baseButton} border border-amber-200/30 bg-black/25 text-amber-100 hover:-translate-y-0.5 hover:bg-amber-100/10 disabled:hover:translate-y-0`,
  danger: `${baseButton} border border-red-300/20 bg-red-400/10 text-red-100 hover:-translate-y-0.5 hover:border-red-300/35 hover:bg-red-400/15 disabled:hover:translate-y-0`,
  ghost:
    'inline-flex min-w-0 w-full items-center justify-center rounded-2xl border border-amber-200/18 bg-black/20 px-4 py-2.5 text-center text-sm font-semibold leading-snug text-amber-100 transition hover:border-amber-200/35 hover:bg-amber-100/10 sm:w-auto',
}
