export default {
  iconLinks: [
    {
      icon: 'github',
      href: 'https://github.com/natemcmaster/CommandLineUtils',
      title: 'GitHub'
    },
    {
      icon: 'box-seam',
      href: 'https://nuget.org/packages/McMaster.Extensions.CommandLineUtils',
      title: 'NuGet'
    }
  ],
  start: () => {
    // Google Analytics
    const script = document.createElement('script');
    script.async = true;
    script.src = 'https://www.googletagmanager.com/gtag/js?id=G-JWDHMFE8XZ';
    document.head.appendChild(script);

    window.dataLayer = window.dataLayer || [];
    function gtag() { dataLayer.push(arguments); }
    gtag('js', new Date());
    gtag('config', 'G-JWDHMFE8XZ');
  }
}
