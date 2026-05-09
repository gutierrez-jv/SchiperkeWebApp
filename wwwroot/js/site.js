document.addEventListener("DOMContentLoaded", () => {
  const header = document.querySelector(".app-header");
  const nav = document.getElementById("mainNavigation");

  const updateHeader = () => {
    header?.classList.toggle("is-scrolled", window.scrollY > 8);
  };

  updateHeader();
  window.addEventListener("scroll", updateHeader, { passive: true });

  nav?.querySelectorAll("a.nav-link, .dropdown-item").forEach((link) => {
    link.addEventListener("click", () => {
      if (window.innerWidth >= 1200 || !nav.classList.contains("show") || !window.bootstrap) {
        return;
      }

      window.bootstrap.Collapse.getOrCreateInstance(nav).hide();
    });
  });

  const appointmentStatusSelect = document.getElementById("Status");
  const cancellationSection = document.getElementById("cancellationSection");

  if (appointmentStatusSelect && cancellationSection) {
    const syncCancellationSection = () => {
      cancellationSection.hidden = appointmentStatusSelect.value !== "Cancelled";
    };

    appointmentStatusSelect.addEventListener("change", syncCancellationSection);
    syncCancellationSection();
  }
});
