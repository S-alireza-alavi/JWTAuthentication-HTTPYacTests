var SUPABASE_URL = 'https://zdgocunjwgxchxbxhytu.supabase.co'
var SUPABASE_KEY =
    'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InpkZ29jdW5qd2d4Y2h4YnhoeXR1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MDgyMzA4ODgsImV4cCI6MjAyMzgwNjg4OH0.nczlRY1bsS4_IJfdezci_uLL4tKag2bDgM0qTxDNcOc'

var supabase = supabase.createClient(SUPABASE_URL, SUPABASE_KEY)

async function logInSubmitted () {
    const email = document.getElementById('log-in-email').value;
    const password = document.getElementById('log-in-password').value;

    const { user, session, error} = await supabase.auth.signIn({
        email: email,
        password: password
    })

    if (error) {
        document.getElementById('generated-token').value = 'Error logging in: ' + error.message;
        console.error('Error logging in:', error)
    }

    setToken(session.access_token);
    setCookie('Token', session.access_token);
    fetchUserDetails();

    // The initial access token and refresh token
    console.log('Initial Access Token:', session.access_token)
    console.log('Initial Refresh Token:', session.refresh_token)
}

function fetchUserDetails () {
    document.getElementById('user-info').innerText = JSON.stringify(supabase.auth.user(), null, 4);
}

function setCookie (name, value) {
    document.cookie = name + "=" + encodeURIComponent(value);
}

function setToken (token) {
    console.log('New token set on the client side', token);
    document.querySelector('#generated-token').value = token;
}

async function resetPassword() {
    const session = supabase.auth.session();
    if (session) {
        const token = session.access_token;
        try {
            const response = await axios.post('/reset-password', {}, {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            });
            alert('Success: ' + response.data.message);
        } catch (error) {
            if (error.response && error.response.status === 403) {
                alert('You are not authorized');
            } else {
                alert('An error occurred: ' + error.message);
            }
        }
    } else {
        alert('You are not authorized');
    }
}