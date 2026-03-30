import NextAuth from "next-auth";
import { JWT } from "next-auth/jwt";
import { DefaultSession } from "next-auth";

declare module "next-auth" {
  interface User {
    // type object user được trả về sau khi đăng nhập thành công, có thể chứa các thông tin như id, name, email, image, roles,...
    roles: string[];
  }
  /**
   * Returned by `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
   */
  interface Session {
    user: {
      roles: string[];
      userId: string;
      userName: string;
    } & DefaultSession["user"];
  }
}

declare module "next-auth/jwt" {
  /** Returned by the `jwt` callback and `getToken`, when using JWT sessions */
  interface JWT {
    roles: string[];
    sub?: string;
    name?: string;
  }
}
