// Yeh config Vercel ko batayega ki kin files par rule lagana hai aur kin par nahi
export const config = {
  matcher: [
    // '/assets' folder aur kisi bhi file jisme '.' (dot) ho usko ignore karo
    '/((?!assets|.*\\..*).*)'
  ]
};

export default function middleware(request) {
  const url = new URL(request.url);
  
  // Ab yeh sirf HTML pages ke URL ko check karega aur lowercase mein badlega
  if (url.pathname !== url.pathname.toLowerCase()) {
    url.pathname = url.pathname.toLowerCase();
    return Response.redirect(url, 308);
  }
}
