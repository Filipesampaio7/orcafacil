export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-3 px-6 text-center">
      <span className="rounded-full border border-accent/30 bg-accent/10 px-3 py-1 text-xs font-medium text-accent">
        Em construção — FASE 1
      </span>
      <h1 className="font-display text-3xl text-text">OrçaFácil</h1>
      <p className="max-w-sm text-sm text-text-muted">
        Estrutura do projeto criada. O fluxo de login (FASE 3) e o dashboard
        (FASE 9) substituirão esta página.
      </p>
    </main>
  );
}
