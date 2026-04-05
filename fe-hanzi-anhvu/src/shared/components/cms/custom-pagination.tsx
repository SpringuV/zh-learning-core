import {
    Pagination,
    PaginationContent,
    PaginationItem,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";
import { Field } from "@/shared/components/ui/field";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
interface PaginationProps {
    currentPage: number;
    totalPages: number;
    itemsPerPage: number;
    startIndex: number;
    endIndex: number;
    totalItems: number;
    totalDocs: number;
    onPageChange: (page: number) => void;
    onItemsPerPageChange: (value: number) => void;
}

export function CustomPagination({
    currentPage,
    totalPages,
    itemsPerPage,
    startIndex,
    endIndex,
    totalItems,
    totalDocs,
    onPageChange,
    onItemsPerPageChange,
}: PaginationProps) {
    const renderPageButtons = () => {
        const maxPages = 3;
        const pages = Math.min(maxPages, totalPages);

        return Array.from({ length: pages }, (_, i) => {
            let pageNum;

            if (totalPages <= maxPages) {
                pageNum = i + 1;
            } else if (currentPage <= Math.ceil(maxPages / 2)) {
                pageNum = i + 1;
            } else if (currentPage >= totalPages - Math.floor(maxPages / 2)) {
                pageNum = totalPages - (maxPages - 1) + i;
            } else {
                pageNum = currentPage - Math.floor(maxPages / 2) + i;
            }

            return (
                <PaginationItem key={pageNum}>
                    <button
                        onClick={() => onPageChange(pageNum)}
                        className={`px-2 sm:px-3 py-1 sm:py-1.5 rounded-lg text-xs sm:text-sm font-medium transition-all duration-200 ease-in-out hover:shadow-sm min-w-7 sm:min-w-9 ${
                            currentPage === pageNum
                                ? "bg-[#007cfc] text-white shadow-md"
                                : "border border-gray-300 text-gray-700 hover:bg-gray-50"
                        }`}
                    >
                        {pageNum}
                    </button>
                </PaginationItem>
            );
        });
    };

    return (
        <div className="mt-3 md:mt-4 px-2 sm:px-4 py-2 sm:py-3 flex sm:items-center justify-between bg-white rounded-lg shadow-sm border border-gray-200 gap-3">
            {/* Left side: Info and items per page selector */}
            <div className="flex flex-col xs:flex-row items-start xs:items-center gap-2 xs:gap-4">
                <div className="text-xs sm:text-sm text-gray-600 whitespace-nowrap">
                    <span className="hidden sm:inline">Hiển thị </span>
                    <span className="font-medium">{startIndex + 1}</span> -{" "}
                    <span className="font-medium">
                        {Math.min(endIndex, totalItems)}
                    </span>{" "}
                    <span className="hidden xs:inline">/ </span>
                    <span className="xs:hidden">trong </span>
                    <span className="font-medium">{totalDocs}</span>
                </div>
                <Field className="flex flex-row! items-center gap-2">
                    <label className="text-xs sm:text-sm text-gray-600 whitespace-nowrap">
                        Hiển thị:
                    </label>
                    <Select
                        value={itemsPerPage.toString()}
                        onValueChange={(value) =>
                            onItemsPerPageChange(Number(value))
                        }
                    >
                        <SelectTrigger className="w-16 sm:w-20 h-8 sm:h-9 text-xs sm:text-sm border border-gray-300 hover:border-[#007cfc] focus:ring-2 focus:ring-[#007cfc]">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="50">50</SelectItem>
                            <SelectItem value="100">100</SelectItem>
                            <SelectItem value="150">150</SelectItem>
                        </SelectContent>
                    </Select>
                    <span className="text-xs sm:text-sm text-gray-600 whitespace-nowrap">
                        / trang
                    </span>
                </Field>
            </div>

            {/* Right side: Pagination controls */}
            <Pagination className="mx-0 w-auto">
                <PaginationContent className="gap-1 sm:gap-2">
                    {/* First page button */}
                    <PaginationItem>
                        <button
                            onClick={() => onPageChange(1)}
                            disabled={currentPage === 1}
                            className="px-2 sm:px-3 py-1 sm:py-1.5 rounded-lg border border-gray-300 text-xs sm:text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200 ease-in-out hover:shadow-sm"
                            title="Trang đầu"
                        >
                            <span className="hidden xs:inline">{"<<"}</span>
                            <span className="xs:hidden">{"<"}</span>
                        </button>
                    </PaginationItem>

                    {/* Previous page */}
                    <PaginationItem>
                        <PaginationPrevious
                            onClick={() =>
                                onPageChange(Math.max(1, currentPage - 1))
                            }
                            className={`px-2 sm:px-3 py-1 sm:py-1.5 text-xs sm:text-sm ${
                                currentPage === 1
                                    ? "opacity-50 cursor-not-allowed"
                                    : "cursor-pointer"
                            }`}
                            aria-disabled={currentPage === 1}
                        />
                    </PaginationItem>

                    {/* Page numbers */}
                    {renderPageButtons()}

                    {/* Next page */}
                    <PaginationItem>
                        <PaginationNext
                            onClick={() =>
                                onPageChange(
                                    Math.min(totalPages, currentPage + 1),
                                )
                            }
                            className={`px-2 sm:px-3 py-1 sm:py-1.5 text-xs sm:text-sm ${
                                currentPage === totalPages
                                    ? "opacity-50 cursor-not-allowed"
                                    : "cursor-pointer"
                            }`}
                            aria-disabled={currentPage === totalPages}
                        />
                    </PaginationItem>
                    <PaginationItem>
                        <button
                            onClick={() => onPageChange(totalPages)}
                            disabled={currentPage === totalPages}
                            className="px-2 sm:px-3 py-1 sm:py-1.5 rounded-lg border border-gray-300 text-xs sm:text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200 ease-in-out hover:shadow-sm"
                            title="Trang cuối"
                        >
                            <span className="hidden xs:inline">{">>"}</span>
                            <span className="xs:hidden">{">"}</span>
                        </button>
                    </PaginationItem>
                </PaginationContent>
            </Pagination>
        </div>
    );
}
