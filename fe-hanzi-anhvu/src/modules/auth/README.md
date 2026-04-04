# Auth Module Structure

Muc tieu: giu auth module de doc, de test, de mo rong.

## Cau truc

- `api/`: goi HTTP thuần den backend (`auth.api.ts`).
- `hooks/`: orchestration va UI state cho auth flow (`use-auth-*.ts`).
- `components/`: UI components cho auth (`login`, `register`, `logout`).
- `types/`: DTO va type theo auth domain.

## Rule 1: UI khong goi API truc tiep

- Page/Component chi goi hooks.
- Hooks moi la noi ket hop mutation + status + message.

## Rule 2: Trang thai auth dung 1 kieu

- Dung chung `AuthUiStatus`: `idle | loading | success | error | warning`.
- Hook tra ve: `status`, `message`, `isLoading`, action handlers.

## Rule 3: Thong bao trang thai ro rang

- Hien thi thong bao truc tiep trong page auth.
- Khong trộn HTML panel va toast side-effect khi khong can thiet.

## Rule 4: Dat ten theo vai tro

- `use-auth-*-orchestrator.ts`: dieu phoi 1 flow (activate/resend/...).
- Tranh ten mơ hồ khong dung voi hanh vi (vi du panel nhung thuc te la toast).

## Rule 5: Mo rong flow moi

Khi them flow auth moi (forgot/reset/change-password), tao:

1. Hook orchestrator rieng trong `hooks/`.
2. Dung lai `AuthUiStatus` + pattern return giong nhau.
3. Reuse block thong bao inline de giu UX nhat quan.
