import { useAuth } from '../features/auth/AuthContext';
import { useNavigate, Navigate } from 'react-router-dom';
import { useState } from 'react';
import Navbar from '../components/NavBar/LoginRegisterNavBar';
import InputField from '../components/Form/InputField';
import LoginRegisterButton from '../components/Form/LoginRegisterButton';
import ImageInputRadio from '../components/Form/ImageInputRadio';
import { showToast } from '../components/Feedback/ToastNotify';

const Register = () => {
    const [role, setRole] = useState(null);
    const [nome, setNome] = useState('');
    const [email, setEmail] = useState('');
    const [senha, setSenha] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    const { register } = useAuth();
    const navigate = useNavigate();

    const { token } = useAuth();

    if (token) {
        return <Navigate to="/dashboard" replace />;
    }

    const handleCadastro = async (e) => {
        e.preventDefault();
        if (!email || !senha || !nome || role === null) {
            setError('Preencha todos os campos!');
            return;
        }
        setIsLoading(true);
        const registrar = await register({ nome, email, senha, tipo: role });
        setIsLoading(false);
        if (registrar.passou) {
            showToast({
                message:
                    'Cadastro realizado com sucesso! Redirecionando para login...',
                type: 'success',
                duration: 4000,
                navigateTo: () => navigate('/', { replace: true }),
            });
        } else {
            setError(`${registrar.retorno}`);
        }
    };

    return (
        <div>
            {
                <>
                    <div className="bg-gradient-to-b from-background-escuro to-background-claro min-h-screen w-screen flex flex-col justify-between">
                        <Navbar />
                        <div className="mt-6 mb-12 w-[452px] rounded-[20px] bg-background-login mx-auto self-center p-9">
                            <form
                                className="flex flex-col"
                                onSubmit={handleCadastro}
                            >
                                <p className="text-3xl font-bold text-titulo-login mb-12 mx-auto self-center">
                                    Cadastro
                                </p>

                                <p className="text-base font-bold text-titulo-login mb-8 mx-auto self-center">
                                    Escolha o tipo de conta
                                </p>

                                <ImageInputRadio
                                    value={role}
                                    onChange={setRole}
                                />

                                <p className="text-sm text-placeholder-login mb-8 mx-1">
                                    {role == null ? (
                                        ''
                                    ) : (
                                        <>
                                            Seja bem-vindo! Complete as
                                            informações a seguir para concluir
                                            seu cadastro como{' '}
                                            {role == 0 ? 'gerente' : 'cliente'}.
                                        </>
                                    )}
                                </p>

                                <div className="gap-y-6 flex flex-col mb-2">
                                    <InputField
                                        label="Nome"
                                        type="text"
                                        value={nome}
                                        onChange={(e) =>
                                            setNome(e.target.value)
                                        }
                                        placeholder="Seu nome completo"
                                        required={true}
                                    />

                                    <InputField
                                        label="Email"
                                        type="email"
                                        value={email}
                                        onChange={(e) =>
                                            setEmail(e.target.value)
                                        }
                                        placeholder="Seu endereço de email"
                                        required={true}
                                    />

                                    <InputField
                                        label="Senha"
                                        type="password"
                                        value={senha}
                                        onChange={(e) =>
                                            setSenha(e.target.value)
                                        }
                                        placeholder="Sua senha"
                                        required={true}
                                    />
                                </div>

                                <div className="flex items-center justify-start">
                                    {error !== '' ? (
                                        <p className="text-dashboard-red-400 text-xs mb-6">
                                            {error}
                                        </p>
                                    ) : (
                                        <div className="mb-8"></div>
                                    )}
                                </div>

                                <LoginRegisterButton
                                    label="CADASTRAR"
                                    isLoading={isLoading}
                                />

                                <span className="mt-4 mb-2 self-start text-[14px] flex items-center gap-x-2">
                                    <p className="text-text-register">
                                        Já tem uma conta?
                                    </p>
                                    <p
                                        onClick={() => navigate('/')}
                                        className="text-button-register hover:underline hover:cursor-pointer font-bold"
                                    >
                                        Entrar
                                    </p>
                                </span>
                            </form>
                        </div>
                    </div>
                </>
            }
        </div>
    );
};

export default Register;
