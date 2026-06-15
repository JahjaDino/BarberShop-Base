import barberShop1 from '../../assets/optimized/barberShop1-gallery.webp'
import beardCut from '../../assets/optimized/beardCut-gallery.webp'
import equipment from '../../assets/optimized/equipment-gallery.webp'
import fadeCut from '../../assets/optimized/fadeCut-gallery.webp'

const galleryItems = [
  { title: 'Fade Cut', image: fadeCut },
  { title: 'Beard Trim', image: beardCut },
  { title: 'Salon Interior', image: barberShop1 },
  { title: 'Premium Equipment', image: equipment },
]

function GallerySection() {
  return (
    <section id="gallery" className="border-t border-amber-200/10 py-16">
      <div className="mx-auto max-w-[1500px] px-5 sm:px-8 lg:px-14 xl:px-20">
        <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
          Galerija
        </p>

        <h2 className="mt-4 max-w-4xl text-4xl font-black leading-tight text-stone-50 sm:text-5xl">
          Pogledajte atmosferu i stil našeg salona.
        </h2>

        <div className="mt-10 grid gap-4 md:grid-cols-2">
          {galleryItems.map((item) => (
            <article
              key={item.title}
              className="group relative h-[300px] overflow-hidden rounded-[28px] border border-amber-200/10 bg-white/[0.03] shadow-[0_0_35px_rgba(0,0,0,0.25)] sm:h-[320px]"
            >
              <img
                src={item.image}
                alt={item.title}
                loading="lazy"
                decoding="async"
                className="h-full w-full object-cover transition duration-700 group-hover:scale-105"
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black/75 via-black/20 to-transparent" />

              <div className="absolute bottom-5 left-5 rounded-2xl border border-amber-200/15 bg-black/35 px-4 py-3 backdrop-blur">
                <p className="text-sm font-bold uppercase tracking-[0.2em] text-amber-100">
                  {item.title}
                </p>
              </div>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

export default GallerySection
