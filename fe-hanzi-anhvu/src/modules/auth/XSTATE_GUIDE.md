# Hướng Dẫn Tạo XState State Machine

## Tổng Quan

XState là thư viện JavaScript/TypeScript để tạo và quản lý state machines. Nó giúp bạn:

- Quản lý state phức tạp một cách có tổ chức
- Xử lý async operations an toàn
- Type-safe với TypeScript
- Dễ test và debug

## Bước 1: Phân Tích Vấn Đề

Trước khi code, hãy phân tích:

### 1.1 Xác định States

States là các trạng thái chính của flow:

```typescript
// Ví dụ: Auth activate flow
type States = "idle" | "activating" | "success" | "error";
```

### 1.2 Xác định Events

Events là các trigger để chuyển đổi state:

```typescript
type Events = { type: "ACTIVATE" } | { type: "RESET" };
```

### 1.3 Xác định Context

Context là data được lưu trữ trong machine:

```typescript
type Context = {
    email: string;
    code: string;
    message: string;
    status: UiStatus;
};
```

### 1.4 Xác định Actions

Actions là logic được thực hiện khi chuyển state:

```typescript
type Actions = {
    setLoading: () => void;
    setSuccess: () => void;
    setError: () => void;
    reset: () => void;
};
```

## Bước 2: Tạo Machine Cơ Bản

### 2.1 Import và Setup

```typescript
import { setup } from "xstate";

const machine = setup({
    types: {
        context: {} as Context,
        events: {} as Events,
    },
}).createMachine({
    id: "myMachine",
    initial: "idle",
    context: {
        email: "",
        code: "",
        message: "",
        status: "idle",
    },
    states: {
        // States sẽ được định nghĩa ở bước sau
    },
});
```

### 2.2 Định Nghĩa States và Transitions

```typescript
states: {
  idle: {
    on: {
      ACTIVATE: {
        target: "activating",
        actions: "setLoading",
      },
    },
  },
  activating: {
    invoke: {
      src: "activateActor",
      onDone: {
        target: "success",
        actions: "setSuccess",
      },
      onError: {
        target: "error",
        actions: "setError",
      },
    },
  },
  success: {
    on: {
      RESET: {
        target: "idle",
        actions: "reset",
      },
    },
  },
  error: {
    on: {
      RESET: "idle",
      RETRY: "activating",
    },
  },
},
```

## Bước 3: Thêm Actions

### 3.1 Tạo Actions Object

```typescript
import { assign } from "xstate";

const actions = {
    setLoading: assign({
        status: "loading",
        message: "Đang xử lý...",
    }),
    setSuccess: assign({
        status: "success",
        message: "Thành công!",
    }),
    setError: assign({
        status: "error",
        message: ({ event }: any) => event.error?.message || "Có lỗi xảy ra",
    }),
    reset: assign({
        status: "idle",
        message: "",
    }),
};
```

### 3.2 Thêm Actions vào Machine

```typescript
const machine = setup({
    types: {
        context: {} as Context,
        events: {} as Events,
    },
    actions, // Thêm actions vào đây
}).createMachine({
    // ... machine config
});
```

## Bước 4: Thêm Actors cho Async Operations

### 4.1 Tạo Actor

```typescript
import { fromPromise } from "xstate";

const actors = {
    activateActor: fromPromise(async ({ input }: any) => {
        // Gọi API ở đây
        const response = await api.activateAccount(input);
        return response.data;
    }),
};
```

### 4.2 Thêm Actors vào Machine

```typescript
const machine = setup({
    types: {
        context: {} as Context,
        events: {} as Events,
    },
    actions,
    actors, // Thêm actors vào đây
}).createMachine({
    // ... machine config
});
```

## Bước 5: Sử dụng trong React

### 5.1 Tạo Custom Hook

```typescript
import { useMachine } from "@xstate/react";

export function useMyMachine(initialContext: Partial<Context>) {
    const [state, send] = useMachine(machine, {
        input: initialContext,
    });

    return {
        state,
        send,
        // Computed values
        isLoading: state.matches("activating"),
        canActivate: state.can({ type: "ACTIVATE" }),
        // Actions
        activate: () => send({ type: "ACTIVATE" }),
        reset: () => send({ type: "RESET" }),
    };
}
```

### 5.2 Sử dụng trong Component

```typescript
function ActivatePage() {
  const { state, activate, reset, isLoading } = useMyMachine({
    email: "user@example.com",
    code: "123456",
  });

  return (
    <div>
      <p>Status: {state.context.status}</p>
      <p>Message: {state.context.message}</p>

      <button onClick={activate} disabled={isLoading}>
        {isLoading ? "Đang kích hoạt..." : "Kích hoạt"}
      </button>

      <button onClick={reset}>Reset</button>
    </div>
  );
}
```

## Bước 6: Testing

### 6.1 Unit Test Machine

```typescript
import { createActor } from "xstate";

describe("My Machine", () => {
    it("should transition to activating on ACTIVATE", () => {
        const actor = createActor(machine);
        actor.start();

        actor.send({ type: "ACTIVATE" });
        expect(actor.getSnapshot().value).toBe("activating");
    });
});
```

### 6.2 Integration Test với Component

```typescript
import { render, screen } from "@testing-library/react";
import { ActivatePage } from "./ActivatePage";

test("shows loading state when activating", () => {
  render(<ActivatePage />);
  // Test logic here
});
```

## Bước 7: Best Practices

### 7.1 File Structure

```
machines/
├── my-machine.ts          # Machine definition
├── my-machine.test.ts     # Tests
└── my-machine.types.ts    # Types (optional)
```

### 7.2 Naming Conventions

- Machine ID: lowercase-with-dashes
- State names: lowercase
- Event types: UPPERCASE_WITH_UNDERSCORES
- Action names: camelCase

### 7.3 Error Handling

```typescript
states: {
  activating: {
    invoke: {
      src: "activateActor",
      onDone: "success",
      onError: {
        target: "error",
        actions: assign({
          error: ({ event }) => event.error,
        }),
      },
    },
  },
}
```

### 7.4 Guards

```typescript
const guards = {
  canActivate: ({ context }) =>
    Boolean(context.email) && Boolean(context.code),
};

states: {
  idle: {
    on: {
      ACTIVATE: {
        target: "activating",
        guard: "canActivate",
      },
    },
  },
}
```

## Bước 8: Advanced Patterns

### 8.1 Hierarchical States

```typescript
states: {
  authenticated: {
    initial: "dashboard",
    states: {
      dashboard: {},
      profile: {},
      settings: {},
    },
  },
}
```

### 8.2 Parallel States

```typescript
const machine = createMachine({
    id: "app",
    initial: "idle",
    states: {
        idle: {
            /* ... */
        },
        loading: {
            type: "parallel", // Tất cả sub-states chạy song song
            states: {
                user: {
                    initial: "loading",
                    states: {
                        loading: {
                            /* Gửi API user */
                        },
                        loaded: {
                            /* User data ready */
                        },
                    },
                },
                posts: {
                    initial: "loading",
                    states: {
                        loading: {
                            /* Gửi API posts */
                        },
                        loaded: {
                            /* Posts data ready */
                        },
                    },
                },
            },
            onDone: "ready", // Chuyển khi cả user và posts đều loaded
        },
        ready: {
            /* App sẵn sàng */
        },
    },
});
```

### 8.3 Delayed Transitions

```typescript
states: {
  success: {
    after: {
      2000: "idle", // Auto transition after 2s
    },
  },
}
```

## Troubleshooting

### Common Issues:

1. **Machine không transition**: Check guard conditions
2. **Actions không chạy**: Ensure actions are defined in setup
3. **Type errors**: Check context and event types
4. **Async issues**: Use proper error handling in actors

### Debug Tips:

- Use XState DevTools: `npm install @xstate/inspect`
- Log state changes: `actor.subscribe(console.log)`
- Test individual transitions first

## Resources

- [XState Docs](https://xstate.js.org/)
- [XState Visualizer](https://stately.ai/viz)
- [XState Examples](https://github.com/statelyai/xstate/tree/main/examples)

Happy state managing! 🎯</content>
<parameter name="filePath">d:\.NET\Project\hanzi_dev\zh-learning-core\fe-hanzi-anhvu\src\modules\auth\XSTATE_GUIDE.md
