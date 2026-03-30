"use client";
import { SessionProvider } from "next-auth/react";

export default function NextAuthWrapper({
  children,
  session,
}: {
  children: React.ReactNode;
  session: any;
}) {
  return (
    <SessionProvider
      session={session}
      refetchInterval={0}
      refetchOnWindowFocus={false}
      basePath="/auth"
    >
      {children}
    </SessionProvider>
  );
}
