export default function middleware(request) {
  const url = new URL(request.url);
  
  // Agar URL ke path mein koi bhi capital letter hai
  if (url.pathname !== url.pathname.toLowerCase()) {
    // Toh usko completely small letters (lowercase) mein convert kar do
    url.pathname = url.pathname.toLowerCase();
    
    // Aur user ko naye sahi URL par bhej do (308 Permanent Redirect)
    return Response.redirect(url, 308);
  }
}
