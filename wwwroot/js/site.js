document.addEventListener("DOMContentLoaded", () => {
  const header = document.querySelector(".app-header");
  const nav = document.getElementById("mainNavigation");

  const updateHeader = () => {
    header?.classList.toggle("is-scrolled", window.scrollY > 8);
  };

  updateHeader();
  window.addEventListener("scroll", updateHeader, { passive: true });

  nav?.querySelectorAll("a.nav-link").forEach((link) => {
    link.addEventListener("click", () => {
      if (window.innerWidth >= 992 || !nav.classList.contains("show") || !window.bootstrap) {
        return;
      }

      window.bootstrap.Collapse.getOrCreateInstance(nav).hide();
    });
  });
});
