import { defineConfig } from "vitepress";

// https://vitepress.vuejs.org/config/app-configs
export default defineConfig({
  title: "Ticketing System",

  themeConfig: {
    sidebar: [
      {
        text: "Overview",
        items: [{ text: "Introduction", link: "/" }],
      },

      {
        text: "Core System",
        items: [
          { text: "Architecture", link: "/architecture" },
          { text: "CQRS", link: "/cqrs" },
          { text: "Domain Model", link: "/domain-model" },
          { text: "Concurrency", link: "/concurrency" },
        ],
      },

      {
        text: "Distributed Systems",
        items: [
          { text: "Redis Locking", link: "/redis-locking" },
          { text: "Outbox Pattern", link: "/outbox-pattern" },
          { text: "Messaging", link: "/messaging" },
        ],
      },
      {
        text: "System",
        items: [{ text: "Testing", link: "/testing" }],
      },
    ],
  },
});
