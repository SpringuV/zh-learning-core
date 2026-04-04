import http from "@/shared/utils/http";

const endpoints = {
    getListUsers: "/search/v1/users",
};

export const usersApi = {
    async getListUsers() {
        return await http.get(endpoints.getListUsers);
    },
};
