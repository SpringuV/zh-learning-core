"use client";

import { Button } from "@/shared/components/ui/button";
import { Home, AlertTriangle } from "lucide-react";
import Link from "next/link";

export default function Page403() {
    return (
        <div className="min-h-screen bg-linear-to-br from-primary/5 via-background to-primary/10 flex items-center justify-center px-4">
            {/* Background decorative elements */}
            <div className="absolute top-0 left-0 w-96 h-96 bg-linear-to-br from-primary/20 to-transparent rounded-full blur-3xl opacity-40" />
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-linear-to-tl from-secondary/20 to-transparent rounded-full blur-3xl opacity-40" />

            {/* Content */}
            <div className="relative z-10 max-w-md text-center">
                {/* 403 Number */}
                <div className="mb-6">
                    <div className="relative inline-block">
                        <div className="text-9xl font-black bg-linear-to-r from-primary to-secondary bg-clip-text text-transparent drop-shadow-lg">
                            403
                        </div>
                        <div className="absolute -top-8 -right-8 bg-primary text-primary-foreground rounded-full p-3 shadow-lg">
                            <AlertTriangle className="w-6 h-6" />
                        </div>
                    </div>
                </div>

                {/* Title */}
                <h1 className="text-4xl font-bold text-foreground mb-3">
                    Bị Từ Chối Truy Cập
                </h1>

                {/* Description */}
                <p className="text-muted-foreground text-lg mb-8 leading-relaxed">
                    Bạn không có quyền truy cập vào trang này. Vui lòng kiểm tra
                    quyền của mình hoặc liên hệ với quản trị viên.
                </p>

                {/* Additional info */}
                <div className="bg-primary/10 border border-primary/20 rounded-lg p-4 mb-8">
                    <p className="text-sm text-muted-foreground">
                        Nếu bạn cho rằng đây là một lỗi, vui lòng liên hệ với
                        đội hỗ trợ của chúng tôi.
                    </p>
                </div>

                {/* Buttons */}
                <div className="flex flex-col sm:flex-row gap-3 justify-center">
                    <Link href="/">
                        <Button
                            size="lg"
                            className="bg-primary hover:bg-primary/90 text-primary-foreground w-full sm:w-auto"
                        >
                            <Home className="mr-2 w-4 h-4" />
                            Trở Về Trang Chủ
                        </Button>
                    </Link>
                    <Link href="/">
                        <Button
                            size="lg"
                            variant="outline"
                            className="border-primary/30 hover:bg-primary/5 w-full sm:w-auto"
                        >
                            Liên Hệ Hỗ Trợ
                        </Button>
                    </Link>
                </div>

                {/* Footer text */}
                <p className="text-xs text-muted-foreground mt-8">
                    Mã lỗi: 403 • AccessdeniedError
                </p>
            </div>
        </div>
    );
}
