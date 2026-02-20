/**
 * Gerador de Lero-Lero para a Landing Page.
 * Gera textos 100% aleatórios a cada chamada.
 */

function pick<T>(arr: T[]): T {
  return arr[Math.floor(Math.random() * arr.length)]
}

function pickN<T>(arr: T[], n: number): T[] {
  const shuffled = [...arr].sort(() => Math.random() - 0.5)
  return shuffled.slice(0, n)
}

function capitalize(s: string): string {
  return s.charAt(0).toUpperCase() + s.slice(1)
}

// ── Bancos de palavras ──────────────────────────────────────────────

const substantivos = [
  'paradigma', 'sinergia', 'metodologia', 'framework', 'ecossistema', 'infraestrutura',
  'plataforma', 'solução', 'estratégia', 'inovação', 'jornada', 'experiência',
  'abordagem', 'tecnologia', 'automação', 'integração', 'otimização', 'convergência',
  'escalabilidade', 'resiliência', 'governança', 'transformação', 'disrupção', 'pipeline',
  'roadmap', 'benchmark', 'throughput', 'sprint', 'workflow', 'mindset',
  'blockchain', 'metaverso', 'tokenização', 'gamificação', 'virtualização', 'monetização',
]

const adjetivos = [
  'disruptivo', 'holístico', 'sinérgico', 'proativo', 'escalável', 'resiliente',
  'inovador', 'sustentável', 'orgânico', 'exponencial', 'ágil', 'robusto',
  'dinâmico', 'assertivo', 'colaborativo', 'inteligente', 'premium', 'next-gen',
  'responsivo', 'granular', 'end-to-end', 'full-stack', 'cloud-native', 'data-driven',
  'omnichannel', 'cross-functional', 'bleeding-edge', 'mission-critical', 'best-in-class', 'world-class',
]

const verbos = [
  'potencializar', 'alavancar', 'democratizar', 'revolucionar', 'orquestrar', 'catalisar',
  'maximizar', 'otimizar', 'transformar', 'impulsionar', 'viabilizar', 'integrar',
  'escalar', 'disruptar', 'empoderar', 'sinergizar', 'monetizar', 'operacionalizar',
  'destravar', 'pivotar', 'iterar', 'prototipar', 'deployar', 'refatorar',
]

const complementos = [
  'com foco em resultados tangíveis', 'de forma sustentável e escalável',
  'para o próximo nível de excelência', 'em tempo real e sem fricção',
  'através de metodologias ágeis comprovadas', 'com inteligência artificial de ponta',
  'no ecossistema digital globalizado', 'com ROI mensurável desde o dia um',
  'usando machine learning avançado', 'na velocidade da luz quântica',
  'com zero downtime garantido', 'no paradigma web 5.0',
  'via APIs RESTful de última geração', 'com criptografia quântica militar',
  'através de microserviços distribuídos', 'em containers orquestrados por IA',
  'usando DevOps turbinado por blockchain', 'com edge computing de nova era',
]

const emojis = ['🚀', '💡', '⚡', '🎯', '🔥', '✨', '🌟', '💎', '🏆', '🛡️', '📊', '🧠', '🤖', '🔮', '💫', '🌈', '🎪', '🎭', '🦄', '🐉']

// ── Geradores ───────────────────────────────────────────────────────

export interface FeatureCard {
  emoji: string
  titulo: string
  descricao: string
}

export interface Plano {
  nome: string
  badge?: string
  preco: string
  items: string[]
}

export interface LeroLeroData {
  heroTitulo1: string
  heroTitulo2: string
  heroDesc: string
  heroCtaPrimario: string
  heroCtaSecundario: string
  featuresTitulo: string
  featureCards: FeatureCard[]
  pricingTitulo: string
  planoBaixo: Plano
  planoPop: Plano
  planoAlto: Plano
  footerTexto: string
}

export function useLeroLero(): LeroLeroData {
  const heroTitulos1 = [
    'Transforme sua', 'Revolucione a', 'Potencialize sua', 'Desbloqueie a',
    'Eleve sua', 'Acelere a', 'Reimagine a', 'Turbine sua',
    'Hackeie a', 'Disrupta sua', 'Escale sua', 'Domine a',
  ]
  const heroTitulos2 = [
    'experiência digital', 'jornada tecnológica', 'presença online', 'estratégia omnichannel',
    'infraestrutura cloud', 'visão de futuro', 'máquina de vendas', 'fábrica de ideias',
    'matrix corporativa', 'galáxia de dados', 'dimensão quântica', 'realidade aumentada',
  ]

  const heroDescricoes = [
    () => `Nossa ${pick(substantivos)} ${pick(adjetivos)} vai ${pick(verbos)} seu negócio ${pick(complementos)}.`,
    () => `Utilizamos ${pick(substantivos)} ${pick(adjetivos)} para ${pick(verbos)} cada aspecto da sua operação ${pick(complementos)}.`,
    () => `A ${pick(substantivos)} ${pick(adjetivos)} que vai ${pick(verbos)} seus resultados ${pick(complementos)}.`,
    () => `Prepare-se para ${pick(verbos)} a ${pick(substantivos)} do seu negócio ${pick(complementos)}.`,
  ]

  function gerarTituloFeature(): string {
    return `${capitalize(pick(substantivos))} ${capitalize(pick(adjetivos))}`
  }

  function gerarDescFeature(): string {
    return `${capitalize(pick(verbos))} a ${pick(substantivos)} ${pick(adjetivos)} da sua empresa ${pick(complementos)}. Resultados ${pick(adjetivos)}s garantidos.`
  }

  function gerarItemPlano(): string {
    return `✓ ${capitalize(pick(substantivos))} ${pick(adjetivos)}`
  }

  return {
    heroTitulo1: pick(heroTitulos1),
    heroTitulo2: pick(heroTitulos2),
    heroDesc: pick(heroDescricoes)(),
    heroCtaPrimario: pick(['Começar Agora', 'Bora Lá', 'Me Convenceu', 'Quero Tudo. LAAÁ ELE', 'Teste Grátis', 'Entrar na Matrix']),
    heroCtaSecundario: pick(['Conhecer Serviços', 'Ver Features', 'Saiba Mais', 'Me Conta Mais', 'Rolar a Página', 'Explorar']),

    featuresTitulo: pick([
      'Como podemos ajudar você?', 'Por que somos incríveis?', 'Nossos superpoderes',
      'O que nos torna únicos?', 'Funcionalidades absurdas', 'Features de outro mundo',
      'Diferenciais incomparáveis', 'Por que existimos?', 'O que fazemos de melhor',
    ]),
    featureCards: pickN(emojis, 3).map(emoji => ({
      emoji,
      titulo: gerarTituloFeature(),
      descricao: gerarDescFeature(),
    })),

    pricingTitulo: pick([
      'Planos de Atendimento', 'Invista no futuro', 'Escolha sua aventura',
      'Tabela de preços aleatórios', 'Quanto custa sonhar?', 'Precificação quântica',
      'Planos & Vibes', 'Tiers de investimento',
    ]),
    planoBaixo: {
      nome: pick(['Starter', 'Básico', 'Seed', 'Lite', 'Micro', 'Free-ish', 'Quase Grátis', 'Estagiário']),
      preco: pick(['29', '49', '69', '79', '99']),
      items: Array.from({ length: 3 }, () => gerarItemPlano()),
    },
    planoPop: {
      nome: pick(['Pro', 'Ninja', 'Turbo', 'Ultra', 'Mega', 'Supremo', 'Lendário', 'Chad']),
      badge: pick(['POPULAR', 'HYPADO', 'TOP 1', 'STONKS', 'O BRABO', 'BEST SELLER', 'TRENDING', 'GOAT']),
      preco: pick(['149', '199', '249', '299', '499']),
      items: Array.from({ length: 4 }, () => gerarItemPlano()),
    },
    planoAlto: {
      nome: pick(['Enterprise', 'Galáctico', 'Divino', 'Cósmico', 'Infinity', 'Thanos', 'Orbital', 'Quantum']),
      preco: pick(['Custom', 'Sob consulta', '∞', 'Ligue agora', 'Nem pergunte', 'Se tem que perguntar...']),
      items: Array.from({ length: 4 }, () => gerarItemPlano()),
    },

    footerTexto: pick([
      'Todos os direitos reservados.', 'Nenhum direito reservado, na verdade.',
      'Feito com café e insônia.', 'Powered by lero-lero.', 'Nenhum dev foi ferido neste processo.',
      'Textos gerados por hamsters treinados.', 'Este site se auto-destruirá em 5 segundos.',
      'As opiniões deste site não representam nada.', 'Bateria de buzzwords recarregada.',
    ]),
  }
}
