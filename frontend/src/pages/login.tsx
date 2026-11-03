import { useState, type FormEvent } from "react";
import { Loader2 } from "lucide-react";

import { BrandMark } from "@/components/app/app-shell";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { SessionUser } from "@/lib/types";

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function LoginPage({ onLogin }: { onLogin: (user: SessionUser) => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({});
  const [submitting, setSubmitting] = useState(false);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const nextErrors: { email?: string; password?: string } = {};
    if (!email.trim()) nextErrors.email = "Enter your work email address.";
    else if (!EMAIL_PATTERN.test(email.trim()))
      nextErrors.email = "That does not look like a valid email address.";
    if (!password) nextErrors.password = "Enter your password.";

    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0) return;

    setSubmitting(true);
    onLogin({ name: email.split("@")[0] || "Operator", email: email.trim() });
  }

  return (
    <main className="grid min-h-dvh lg:grid-cols-2">
      <div className="hidden flex-col justify-between border-r bg-sidebar p-10 lg:flex">
        <BrandMark />
        <div className="max-w-md">
          <h1 className="text-3xl font-semibold tracking-tight text-balance text-foreground">
            Network health across the Orange Jordan field estate, in one place.
          </h1>
          <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
            Tower telemetry, incident detection and carbon reporting for operations teams.
          </p>
        </div>
        <p className="text-xs text-muted-foreground">Operational access for authorised users.</p>
      </div>

      <div className="flex items-center justify-center px-6 py-12">
        <div className="w-full max-w-sm">
          <div className="lg:hidden">
            <BrandMark />
          </div>
          <h2 className="mt-8 text-xl font-semibold tracking-tight text-foreground lg:mt-0">
            Sign in
          </h2>
          <p className="mt-1 text-sm text-muted-foreground">Use your Orange Jordan work account.</p>

          <form className="mt-8 space-y-5" noValidate onSubmit={handleSubmit}>
            <div className="space-y-2">
              <Label htmlFor="email">Work email</Label>
              <Input
                aria-describedby={errors.email ? "email-error" : undefined}
                aria-invalid={Boolean(errors.email)}
                autoComplete="email"
                id="email"
                onChange={(event) => setEmail(event.target.value)}
                placeholder="you@orange.jo"
                type="email"
                value={email}
              />
              {errors.email ? (
                <p className="text-sm text-danger" id="email-error">
                  {errors.email}
                </p>
              ) : null}
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input
                aria-describedby={errors.password ? "password-error" : undefined}
                aria-invalid={Boolean(errors.password)}
                autoComplete="current-password"
                id="password"
                onChange={(event) => setPassword(event.target.value)}
                type="password"
                value={password}
              />
              {errors.password ? (
                <p className="text-sm text-danger" id="password-error">
                  {errors.password}
                </p>
              ) : null}
            </div>

            <Button className="w-full" disabled={submitting} type="submit">
              {submitting ? (
                <>
                  <Loader2 aria-hidden="true" className="size-4 animate-spin" />
                  Signing in
                </>
              ) : (
                "Sign in"
              )}
            </Button>
          </form>

          <p className="mt-6 text-xs text-muted-foreground">
            Use your organisation account to access the operations workspace.
          </p>
        </div>
      </div>
    </main>
  );
}
