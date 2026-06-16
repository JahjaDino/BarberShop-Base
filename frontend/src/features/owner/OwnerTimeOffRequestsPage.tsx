import { useEffect, useState } from 'react'
import {
  approveOwnerTimeOffRequest,
  getOwnerTimeOffRequests,
  rejectOwnerTimeOffRequest,
  type OwnerTimeOffRequestDto,
  type OwnerTimeOffStatus,
} from '../../api/ownerTimeOffApi'
import AppCard from '../../components/common/AppCard'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'

function formatDateTime(value: string) {
  const date = new Date(value)

  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleString('bs-BA', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      })
}

function translateStatus(status: OwnerTimeOffStatus) {
  const labels: Record<OwnerTimeOffStatus, string> = {
    PENDING: 'Na čekanju',
    APPROVED: 'Odobreno',
    REJECTED: 'Odbijeno',
  }

  return labels[status] ?? status
}

function getStatusTone(status: OwnerTimeOffStatus) {
  if (status === 'APPROVED') return 'success'
  if (status === 'PENDING') return 'warning'
  if (status === 'REJECTED') return 'danger'
  return 'neutral'
}

function OwnerTimeOffRequestsPage() {
  const [requests, setRequests] = useState<OwnerTimeOffRequestDto[]>([])
  const [reviewNotes, setReviewNotes] = useState<Record<number, string>>({})
  const [activeRequestId, setActiveRequestId] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')

  async function loadRequests() {
    try {
      setIsLoading(true)
      setError('')

      const response = await getOwnerTimeOffRequests()
      setRequests(response.items)
    } catch (requestError) {
      setRequests([])
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Zahtjevi za odsustvo trenutno nisu dostupni.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadRequests()
  }, [])

  async function handleReview(timeOffId: number, action: 'approve' | 'reject') {
    try {
      setActiveRequestId(timeOffId)
      setError('')
      setSuccessMessage('')

      const reviewNote = reviewNotes[timeOffId]
      const updatedRequest =
        action === 'approve'
          ? await approveOwnerTimeOffRequest(timeOffId, reviewNote)
          : await rejectOwnerTimeOffRequest(timeOffId, reviewNote)

      setRequests((currentRequests) =>
        currentRequests.map((request) =>
          request.timeOffId === timeOffId ? updatedRequest : request,
        ),
      )
      setSuccessMessage(
        action === 'approve'
          ? 'Zahtjev za odsustvo je odobren.'
          : 'Zahtjev za odsustvo je odbijen.',
      )
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Zahtjev za odsustvo nije moguće obraditi.',
      )
    } finally {
      setActiveRequestId(null)
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Vlasnički prostor"
        title="Odsustva"
        subtitle="Pregledajte i obradite zahtjeve frizera za odsustvo."
      />

      <SectionCard>
        {error && (
          <p className="mb-5 rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
            {error}
          </p>
        )}

        {successMessage && (
          <p className="mb-5 rounded-2xl border border-emerald-300/20 bg-emerald-300/10 px-4 py-3 text-sm font-semibold text-emerald-100">
            {successMessage}
          </p>
        )}

        {isLoading ? (
          <p className="text-sm text-stone-300">Učitavanje zahtjeva...</p>
        ) : requests.length === 0 ? (
          <EmptyState
            title="Nema zahtjeva za odsustvo."
            description="Kada frizer pošalje zahtjev, bit će prikazan ovdje."
          />
        ) : (
          <div className="grid gap-4">
            {requests.map((request) => (
              <AppCard key={request.timeOffId} variant="subtle">
                <div className="grid min-w-0 gap-5 lg:grid-cols-[minmax(0,1fr)_220px] lg:items-start">
                  <div className="min-w-0">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                      <div className="min-w-0">
                        <p className="break-words text-xs font-semibold uppercase tracking-[0.18em] text-amber-200/65 sm:tracking-[0.22em]">
                          {request.employeeName}
                        </p>
                        <h3 className="mt-2 break-words text-xl font-black text-stone-50">
                          {formatDateTime(request.startDate)} -{' '}
                          {formatDateTime(request.endDate)}
                        </h3>
                      </div>
                      <StatusBadge
                        label={translateStatus(request.status)}
                        tone={getStatusTone(request.status)}
                      />
                    </div>

                    <p className="mt-4 break-words text-sm leading-6 text-stone-300">
                      {request.reason || 'Razlog nije unesen.'}
                    </p>

                    {request.reviewNote && (
                      <p className="mt-3 break-words text-sm text-stone-500">
                        Napomena: {request.reviewNote}
                      </p>
                    )}
                  </div>

                  {request.status === 'PENDING' && (
                    <div className="grid min-w-0 gap-3">
                      <textarea
                        value={reviewNotes[request.timeOffId] ?? ''}
                        onChange={(event) =>
                          setReviewNotes((currentNotes) => ({
                            ...currentNotes,
                            [request.timeOffId]: event.target.value,
                          }))
                        }
                        rows={3}
                        className="w-full min-w-0 resize-none rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-100 outline-none transition focus:border-amber-200/35"
                        placeholder="Napomena za frizera"
                      />

                      <button
                        type="button"
                        disabled={activeRequestId === request.timeOffId}
                        onClick={() => handleReview(request.timeOffId, 'approve')}
                        className={buttonStyles.primary}
                      >
                        Odobri
                      </button>

                      <button
                        type="button"
                        disabled={activeRequestId === request.timeOffId}
                        onClick={() => handleReview(request.timeOffId, 'reject')}
                        className={buttonStyles.danger}
                      >
                        Odbij
                      </button>
                    </div>
                  )}
                </div>
              </AppCard>
            ))}
          </div>
        )}
      </SectionCard>
    </div>
  )
}

export default OwnerTimeOffRequestsPage
