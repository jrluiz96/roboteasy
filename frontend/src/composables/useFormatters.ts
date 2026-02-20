/**
 * Composable com funções utilitárias de formatação
 * reutilizadas em diversas páginas do sistema.
 */
export function useFormatters() {
  /** Ex: "17/02/2026, 15:30" */
  function formatDate(d: string): string {
    return new Date(d).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  /** Ex: "17/02/2026" */
  function formatDateShort(d: string): string {
    return new Date(d).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    })
  }

  /** Ex: "15:30" */
  function formatTime(d: string): string {
    return new Date(d).toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  /** Hora se hoje, dd/mm se outro dia */
  function formatPreview(d: string): string {
    const now = new Date()
    const dt = new Date(d)
    return dt.toDateString() === now.toDateString()
      ? formatTime(d)
      : dt.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })
  }

  /** Ex: "2h15min", "35min", "12s", "agora" */
  function formatDuration(seconds: number | null): string {
    if (!seconds) return '-'
    const h = Math.floor(seconds / 3600)
    const m = Math.floor((seconds % 3600) / 60)
    const s = seconds % 60
    if (h > 0) return `${h}h ${m}m ${s}s`
    return m > 0 ? `${m}m ${s}s` : `${s}s`
  }

  /** Ex: "agora", "5min", "2h15min" (desde uma data ISO) */
  function formatElapsed(d: string): string {
    const mins = Math.floor((Date.now() - new Date(d).getTime()) / 60000)
    if (mins < 1) return 'agora'
    if (mins < 60) return `${mins}min`
    const h = Math.floor(mins / 60)
    return `${h}h${mins % 60}min`
  }

  /** Ex: "MA" para "Maria Antonia" */
  function initials(name: string): string {
    return name
      .split(' ')
      .map((s: string) => s[0])
      .slice(0, 2)
      .join('')
      .toUpperCase()
  }

  return {
    formatDate,
    formatDateShort,
    formatTime,
    formatPreview,
    formatDuration,
    formatElapsed,
    initials,
  }
}
