import { useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import './App.css'

const apiBaseUrl = '/api'
const authTokenStorageKey = 'rh_folha_auth_token'
const authUserStorageKey = 'rh_folha_auth_user'

type AuthUser = {
  id: string
  companyId: string | null
  fullName: string
  email: string
  role: string
}

type AuthSession = {
  token: string
  user: AuthUser
  expiresAt: string
}

function getStoredSession(): AuthSession | null {
  const token = localStorage.getItem(authTokenStorageKey)
  const userJson = localStorage.getItem(authUserStorageKey)
  if (!token || !userJson) return null

  try {
    return { token, user: JSON.parse(userJson) as AuthUser, expiresAt: '' }
  } catch {
    localStorage.removeItem(authTokenStorageKey)
    localStorage.removeItem(authUserStorageKey)
    return null
  }
}

function storeSession(session: AuthSession) {
  localStorage.setItem(authTokenStorageKey, session.token)
  localStorage.setItem(authUserStorageKey, JSON.stringify(session.user))
}

function clearStoredSession() {
  localStorage.removeItem(authTokenStorageKey)
  localStorage.removeItem(authUserStorageKey)
}

function apiFetch(path: string, init: RequestInit = {}) {
  const headers = new Headers(init.headers)
  const token = localStorage.getItem(authTokenStorageKey)

  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  return fetch(path.startsWith('http') ? path : `${apiBaseUrl}${path}`, {
    ...init,
    headers,
  })
}

async function readApiError(response: Response, fallback: string) {
  try {
    const data = await response.json()
    if (typeof data?.message === 'string' && data.message.trim()) {
      return data.message
    }

    if (typeof data?.title === 'string' && data.title.trim()) {
      return data.title
    }
  } catch {
    // Mantem a mensagem padrao quando a API nao retorna JSON.
  }

  return fallback
}

type View =
  | 'dashboard'
  | 'employees'
  | 'structure'
  | 'rubrics'
  | 'periods'
  | 'entries'
  | 'conference'
  | 'reports'
  | 'settings'

type Company = {
  id: string
  legalName: string
  tradeName: string | null
  documentNumber: string
  isActive: boolean
}

type Department = {
  id: string
  companyId: string
  companyName: string
  name: string
  internalCode: string
  isActive: boolean
}

type JobPosition = {
  id: string
  companyId: string
  companyName: string
  name: string
  internalCode: string
  cbo: string | null
  isActive: boolean
}

type Employee = {
  id: string
  companyId: string
  departmentId: string
  departmentName: string
  jobPositionId: string
  jobPositionName: string
  responsibleEmployeeId: string | null
  responsibleEmployeeName: string | null
  registration: string
  name: string
  documentNumber: string
  admissionDate: string
  compensationModel: string
  baseSalary: number
  productionUnitValue: number
  photoUrl: string | null
  status: string
}

type EmployeeEvent = {
  id: string
  companyId: string
  employeeId: string
  eventDate: string
  type: string
  title: string
  description: string
  responsible: string
  status: string
  createdAt: string
}

type Rubric = {
  id: string
  companyId: string
  code: string
  name: string
  type: string
  eSocialNature: string | null
  allowsManualEntry: boolean
  allowsMassEntry: boolean
  allowsFixedEntry: boolean
  isActive: boolean
  currentValidity: RubricValidity | null
}

type RubricValidity = {
  id: string
  startsOn: string
  endsOn: string | null
  incidenceInss: boolean
  incidenceFgts: boolean
  incidenceIrrf: boolean
  incidenceDsr: boolean
  calculationMethod: string
  calculationBase: string
  isActive: boolean
}

type PayrollPeriod = {
  id: string
  companyId: string
  year: number
  month: number
  code: string
  startsOn: string
  endsOn: string
  status: string
  openedAt: string
  closedAt: string | null
}

type PayrollEntry = {
  id: string
  companyId: string
  payrollPeriodId: string
  payrollPeriodCode: string
  employeeId: string
  employeeRegistration: string
  employeeName: string
  rubricId: string
  rubricCode: string
  rubricName: string
  rubricType: string
  entryDate: string
  amount: number
  quantity: number | null
  reference: string | null
  notes: string | null
  origin: string
  status: string
}

type FixedPayrollEntry = {
  id: string
  companyId: string
  employeeId: string
  employeeRegistration: string
  employeeName: string
  rubricId: string
  rubricCode: string
  rubricName: string
  rubricType: string
  startsOn: string
  endsOn: string | null
  amount: number
  quantity: number | null
  notes: string | null
  isActive: boolean
}

type PayrollCalculation = {
  id: string
  payrollPeriodId: string
  periodCode: string
  calculatedAt: string
  totalProventos: number
  totalDescontos: number
  totalLiquido: number
  employeeCount: number
  status: string
}

type PayrollCalculationItem = {
  id: string
  employeeId: string
  employeeRegistration: string
  employeeName: string
  rubricId: string
  rubricCode: string
  rubricName: string
  rubricType: string
  origin: string
  amount: number
  quantity: number | null
  baseAmount: number | null
  calculationRate: number | null
}

type PayrollCalculationDetail = PayrollCalculation & {
  items: PayrollCalculationItem[]
}

type EmployeePayrollSummary = {
  employeeId: string
  employee: string
  registration: string
  name: string
  proventos: number
  descontos: number
  liquido: number
  baseFgts: number
  fgts: number
  baseInss: number
  inss: number
  baseIrrf: number
  irrf: number
  items: number
}

type StatutoryTableRange = {
  id: string
  lowerLimit: number
  upperLimit: number | null
  ratePercent: number
  deductionAmount: number
}

type StatutoryTable = {
  id: string
  companyId: string
  type: string
  name: string
  startsOn: string
  endsOn: string | null
  notes: string | null
  isActive: boolean
  ranges: StatutoryTableRange[]
}

type ProductionRate = {
  id: string
  productionProductId: string | null
  productReference: string | null
  productDescription: string | null
  productionOperationId: string | null
  operationName: string | null
  productionCellId: string | null
  cellName: string | null
  departmentId: string | null
  departmentName: string | null
  jobPositionId: string | null
  jobPositionName: string | null
  unitValue: number
  minimumQuantity: number | null
  maximumQuantity: number | null
  status: string
  notes: string | null
}

type ProductionRateTable = {
  id: string
  companyId: string
  name: string
  effectiveFrom: string
  effectiveTo: string | null
  status: string
  notes: string | null
  rates: ProductionRate[]
}

type SystemUser = {
  id: string
  companyId: string | null
  fullName: string
  email: string
  role: string
  isActive: boolean
  lastLoginAt: string | null
}

type AuditLog = {
  id: string
  createdAt: string
  userName: string
  userEmail: string
  userRole: string
  action: string
  entityName: string
  entityId: string | null
  description: string
  ipAddress: string | null
}

type DapicIntegration = {
  id: string
  companyId: string
  name: string
  baseUrl: string
  externalCompanyIdentifier: string
  accessTokenExpiresAt: string | null
  lastSyncAt: string | null
  status: string
  lastError: string | null
}

type DapicSyncLog = {
  id: string
  resource: string
  startedAt: string
  finishedAt: string | null
  status: string
  pageCount: number
  recordsRead: number
  recordsCreated: number
  recordsUpdated: number
  recordsIgnored: number
  errorMessage: string | null
}

type DapicEmployee = {
  id: string
  companyId: string
  externalId: string
  name: string
  fantasyName: string | null
  displayName: string | null
  status: string
  isIgnored: boolean
  linkStatus: string
  employeeId: string | null
  employeeRegistration: string | null
  employeeName: string | null
  linkedAt: string | null
  ignoredAt: string | null
  ignoredReason: string | null
  lastSyncedAt: string
}

type DapicProduct = {
  id: string
  companyId: string
  externalId: string
  reference: string
  factoryDescription: string
  status: string
  lastSyncedAt: string
}

type DapicNamedProduction = {
  id: string
  companyId: string
  externalId: string
  name: string
  description: string | null
  status: string
  lastSyncedAt: string
}

type DapicProductionOrder = {
  id: string
  companyId: string
  externalId: string
  number: string | null
  description: string | null
  status: string
  rawStatus: string | null
  issueDate: string | null
  startDate: string | null
  endDate: string | null
  lastSyncedAt: string
}

type ProductionCatalogProduct = {
  id: string
  companyId: string
  reference: string
  factoryDescription: string
  status: string
}

type ProductionCatalogNamed = {
  id: string
  companyId: string
  name: string
  description: string | null
  status: string
}

type ProductionCatalogOrder = {
  id: string
  companyId: string
  number: string | null
  description: string | null
  status: string
  issueDate: string | null
}

type ProductionCatalogs = {
  products: ProductionCatalogProduct[]
  operations: ProductionCatalogNamed[]
  cells: ProductionCatalogNamed[]
  orders: ProductionCatalogOrder[]
}

type ProductionEntry = {
  id: string
  companyId: string
  payrollPeriodId: string
  payrollPeriodCode: string
  employeeId: string
  employeeRegistration: string
  employeeName: string
  productionDate: string
  productionOrderId: string | null
  orderNumber: string | null
  productionProductId: string
  productReference: string
  productDescription: string
  productionOperationId: string
  operationName: string
  productionCellId: string | null
  cellName: string | null
  quantity: number
  unitValue: number
  totalAmount: number
  rateSource: string
  productionRateId: string | null
  origin: string
  status: string
  notes: string | null
  approvedAt: string | null
}

type DapicConferenceTab = 'employees' | 'products' | 'operations' | 'cells' | 'orders'
type CreateLinkedEmployeePayload = {
  departmentId: string
  jobPositionId: string
  registration: string
  name: string
  documentNumber: string
  admissionDate: string
  compensationModel: string
  baseSalary: number
  productionUnitValue: number
}
type LoadState = 'loading' | 'ready' | 'error'
type FeedbackType = 'success' | 'error' | 'warning'

type FeedbackMessage = {
  text: string
  type: FeedbackType
}

type SelectOption = {
  value: string
  label: string
  description?: string
}

function App() {
  const [view, setView] = useState<View>('dashboard')
  const [session, setSession] = useState<AuthSession | null>(() => getStoredSession())
  const [loginMessage, setLoginMessage] = useState('')
  const [companies, setCompanies] = useState<Company[]>([])
  const [departments, setDepartments] = useState<Department[]>([])
  const [jobPositions, setJobPositions] = useState<JobPosition[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [rubrics, setRubrics] = useState<Rubric[]>([])
  const [payrollPeriods, setPayrollPeriods] = useState<PayrollPeriod[]>([])
  const [payrollEntries, setPayrollEntries] = useState<PayrollEntry[]>([])
  const [fixedPayrollEntries, setFixedPayrollEntries] = useState<FixedPayrollEntry[]>([])
  const [payrollCalculations, setPayrollCalculations] = useState<PayrollCalculation[]>([])
  const [payrollCalculationDetail, setPayrollCalculationDetail] = useState<PayrollCalculationDetail | null>(null)
  const [statutoryTables, setStatutoryTables] = useState<StatutoryTable[]>([])
  const [productionRateTables, setProductionRateTables] = useState<ProductionRateTable[]>([])
  const [productionEntries, setProductionEntries] = useState<ProductionEntry[]>([])
  const [productionCatalogs, setProductionCatalogs] = useState<ProductionCatalogs>({
    products: [],
    operations: [],
    cells: [],
    orders: [],
  })
  const [systemUsers, setSystemUsers] = useState<SystemUser[]>([])
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([])
  const [dapicIntegrations, setDapicIntegrations] = useState<DapicIntegration[]>([])
  const [dapicLogs, setDapicLogs] = useState<DapicSyncLog[]>([])
  const [dapicEmployees, setDapicEmployees] = useState<DapicEmployee[]>([])
  const [dapicProducts, setDapicProducts] = useState<DapicProduct[]>([])
  const [dapicOperations, setDapicOperations] = useState<DapicNamedProduction[]>([])
  const [dapicCells, setDapicCells] = useState<DapicNamedProduction[]>([])
  const [dapicOrders, setDapicOrders] = useState<DapicProductionOrder[]>([])
  const [loadState, setLoadState] = useState<LoadState>('loading')
  const [message, setMessage] = useState<FeedbackMessage | null>(null)
  const [messageModal, setMessageModal] = useState('')
  const [dapicBusyMessage, setDapicBusyMessage] = useState<string | null>(null)
  const [dapicOrderStartDate, setDapicOrderStartDate] = useState(() => currentMonthStart())
  const [dapicOrderEndDate, setDapicOrderEndDate] = useState(() => todayInputDate())

  const activeCompany = companies[0]

  function showMessage(text: string, type: FeedbackType = inferFeedbackType(text)) {
    setMessage(text ? { text, type } : null)
  }

  const totals = useMemo(
    () => ({
      companies: companies.length,
      departments: departments.length,
      jobPositions: jobPositions.length,
      employees: employees.length,
      rubrics: rubrics.length,
      payrollPeriods: payrollPeriods.length,
      payrollEntries: payrollEntries.length,
      fixedPayrollEntries: fixedPayrollEntries.length,
      calculatedPayrolls: payrollCalculations.length,
      openPayrollPeriods: payrollPeriods.filter((period) => isPeriodOpen(period.status)).length,
      monthlyPayroll: employees.reduce((total, employee) => total + employee.baseSalary, 0),
      productionWorkers: employees.filter((employee) => employee.compensationModel === 'producao').length,
    }),
    [
      companies,
      departments,
      employees,
      fixedPayrollEntries,
      jobPositions,
      payrollCalculations,
      payrollEntries,
      payrollPeriods,
      rubrics,
    ],
  )

  async function loadData() {
    if (!session) return

    setLoadState('loading')
    try {
      const [
        companiesResponse,
        departmentsResponse,
        jobPositionsResponse,
        employeesResponse,
        rubricsResponse,
        payrollPeriodsResponse,
        payrollEntriesResponse,
        fixedPayrollEntriesResponse,
        payrollCalculationsResponse,
        statutoryTablesResponse,
        productionRateTablesResponse,
        productionEntriesResponse,
        productionCatalogsResponse,
      ] = await Promise.all([
          apiFetch('/companies'),
          apiFetch('/departments'),
          apiFetch('/job-positions'),
          apiFetch('/employees'),
          apiFetch('/rubrics'),
          apiFetch('/payroll-periods'),
          apiFetch('/payroll-entries'),
          apiFetch('/fixed-payroll-entries'),
          apiFetch('/payroll-calculations'),
          apiFetch('/statutory-tables'),
          apiFetch('/production-rate-tables'),
          apiFetch('/production-entries'),
          apiFetch('/production-entries/catalogs'),
        ])

      if (
        companiesResponse.status === 401 ||
        departmentsResponse.status === 401 ||
        jobPositionsResponse.status === 401 ||
        employeesResponse.status === 401 ||
        rubricsResponse.status === 401 ||
        payrollPeriodsResponse.status === 401 ||
        payrollEntriesResponse.status === 401 ||
        fixedPayrollEntriesResponse.status === 401 ||
        payrollCalculationsResponse.status === 401 ||
        statutoryTablesResponse.status === 401 ||
        productionRateTablesResponse.status === 401 ||
        productionEntriesResponse.status === 401 ||
        productionCatalogsResponse.status === 401
      ) {
        handleLogout()
        return
      }

      if (
        !companiesResponse.ok ||
        !departmentsResponse.ok ||
        !jobPositionsResponse.ok ||
        !employeesResponse.ok ||
        !rubricsResponse.ok ||
        !payrollPeriodsResponse.ok ||
        !payrollEntriesResponse.ok ||
        !fixedPayrollEntriesResponse.ok ||
        !payrollCalculationsResponse.ok ||
        !statutoryTablesResponse.ok ||
        !productionRateTablesResponse.ok ||
        !productionEntriesResponse.ok ||
        !productionCatalogsResponse.ok
      ) {
        throw new Error('Falha ao carregar cadastros')
      }

      setCompanies(await companiesResponse.json())
      setDepartments(await departmentsResponse.json())
      setJobPositions(await jobPositionsResponse.json())
      setEmployees(await employeesResponse.json())
      setRubrics(await rubricsResponse.json())
      setPayrollPeriods(await payrollPeriodsResponse.json())
      setPayrollEntries(await payrollEntriesResponse.json())
      setFixedPayrollEntries(await fixedPayrollEntriesResponse.json())
      setPayrollCalculations(await payrollCalculationsResponse.json())
      setStatutoryTables(await statutoryTablesResponse.json())
      setProductionRateTables(await productionRateTablesResponse.json())
      setProductionEntries(await productionEntriesResponse.json())
      setProductionCatalogs(await productionCatalogsResponse.json())
      if (isAdmin(session.user.role)) {
        const [
          usersResponse,
          auditLogsResponse,
          dapicIntegrationsResponse,
          dapicEmployeesResponse,
          dapicProductsResponse,
          dapicOperationsResponse,
          dapicCellsResponse,
          dapicOrdersResponse,
        ] = await Promise.all([
          apiFetch('/users'),
          apiFetch('/audit-logs'),
          apiFetch('/integrations/dapic'),
          apiFetch('/integrations/dapic/employees'),
          apiFetch('/integrations/dapic/products'),
          apiFetch('/integrations/dapic/operations'),
          apiFetch('/integrations/dapic/cells'),
          apiFetch('/integrations/dapic/orders'),
        ])

        if (
          !usersResponse.ok ||
          !auditLogsResponse.ok ||
          !dapicIntegrationsResponse.ok ||
          !dapicEmployeesResponse.ok ||
          !dapicProductsResponse.ok ||
          !dapicOperationsResponse.ok ||
          !dapicCellsResponse.ok ||
          !dapicOrdersResponse.ok
        ) {
          throw new Error('Falha ao carregar configuracoes administrativas')
        }

        setSystemUsers(await usersResponse.json())
        setAuditLogs(await auditLogsResponse.json())
        const integrations = (await dapicIntegrationsResponse.json()) as DapicIntegration[]
        setDapicIntegrations(integrations)
        setDapicEmployees(await dapicEmployeesResponse.json())
        setDapicProducts(await dapicProductsResponse.json())
        setDapicOperations(await dapicOperationsResponse.json())
        setDapicCells(await dapicCellsResponse.json())
        setDapicOrders(await dapicOrdersResponse.json())

        if (integrations[0]) {
          const dapicLogsResponse = await apiFetch(`/integrations/dapic/${integrations[0].id}/logs`)
          if (!dapicLogsResponse.ok) {
            throw new Error('Falha ao carregar logs da integracao Dapic')
          }
          setDapicLogs(await dapicLogsResponse.json())
        } else {
          setDapicLogs([])
        }
      } else {
        setSystemUsers([])
        setAuditLogs([])
        setDapicIntegrations([])
        setDapicLogs([])
        setDapicEmployees([])
        setDapicProducts([])
        setDapicOperations([])
        setDapicCells([])
        setDapicOrders([])
      }
      setLoadState('ready')
    } catch {
      setLoadState('error')
    }
  }

  useEffect(() => {
    void loadData()
  }, [session])

  async function handleLoginSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const data = new FormData(event.currentTarget)

    try {
      const response = await fetch(`${apiBaseUrl}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: data.get('email'),
          password: data.get('password'),
        }),
      })

      if (!response.ok) {
        throw new Error('Credenciais invalidas')
      }

      const nextSession = (await response.json()) as AuthSession
      storeSession(nextSession)
      setSession(nextSession)
      setLoginMessage('')
    } catch {
      setLoginMessage('E-mail ou senha invalidos.')
    }
  }

  function handleLogout() {
    clearStoredSession()
    setSession(null)
    setCompanies([])
    setDepartments([])
    setJobPositions([])
    setEmployees([])
    setRubrics([])
    setPayrollPeriods([])
    setPayrollEntries([])
    setFixedPayrollEntries([])
    setPayrollCalculations([])
    setPayrollCalculationDetail(null)
    setStatutoryTables([])
    setProductionRateTables([])
    setProductionEntries([])
    setProductionCatalogs({ products: [], operations: [], cells: [], orders: [] })
    setSystemUsers([])
    setAuditLogs([])
    setDapicIntegrations([])
    setDapicLogs([])
    setDapicEmployees([])
    setDapicProducts([])
    setDapicOperations([])
    setDapicCells([])
    setDapicOrders([])
    setLoadState('loading')
  }

  async function postData(path: string, payload: unknown) {
    const response = await apiFetch(path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      throw new Error(await readApiError(response, 'Nao foi possivel salvar o cadastro'))
    }

    await loadData()
  }

  async function postAction(path: string) {
    const response = await apiFetch(path, {
      method: 'POST',
    })

    if (!response.ok) {
      throw new Error(await readApiError(response, 'Nao foi possivel executar a acao'))
    }

    await loadData()
  }

  async function handleDepartmentSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    try {
      await postData('/departments', {
        companyId: activeCompany.id,
        name: data.get('name'),
        internalCode: data.get('internalCode'),
      })
      form?.reset()
      showMessage('Setor cadastrado.')
    } catch {
      showMessage('Nao foi possivel cadastrar o setor.')
    }
  }

  async function handleJobPositionSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    try {
      await postData('/job-positions', {
        companyId: activeCompany.id,
        name: data.get('name'),
        internalCode: data.get('internalCode'),
        cbo: data.get('cbo'),
      })
      form?.reset()
      showMessage('Cargo cadastrado.')
    } catch {
      showMessage('Nao foi possivel cadastrar o cargo.')
    }
  }

  async function handleEmployeeSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany || departments.length === 0 || jobPositions.length === 0) return

    const form = event.currentTarget
    const data = new FormData(form)
    const compensationModel = String(data.get('compensationModel'))

    try {
      await postData('/employees', {
        companyId: activeCompany.id,
        departmentId: data.get('departmentId'),
        jobPositionId: data.get('jobPositionId'),
        registration: data.get('registration'),
        name: data.get('name'),
        documentNumber: data.get('documentNumber'),
        admissionDate: data.get('admissionDate'),
        compensationModel,
        baseSalary: Number(data.get('baseSalary') || 0),
        productionUnitValue: Number(data.get('productionUnitValue') || 0),
        responsibleEmployeeId: data.get('responsibleEmployeeId') || null,
        photoUrl: null,
      })
      form?.reset()
      showMessage('Colaborador cadastrado.')
    } catch {
      showMessage('Nao foi possivel cadastrar o colaborador.')
    }
  }

  async function handleRubricSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    try {
      await postData('/rubrics', {
        companyId: activeCompany.id,
        code: data.get('code'),
        name: data.get('name'),
        type: data.get('type'),
        description: data.get('description'),
        eSocialNature: data.get('eSocialNature'),
        allowsManualEntry: data.get('allowsManualEntry') === 'on',
        allowsMassEntry: data.get('allowsMassEntry') === 'on',
        allowsFixedEntry: data.get('allowsFixedEntry') === 'on',
        startsOn: data.get('startsOn'),
        incidenceInss: data.get('incidenceInss') === 'on',
        incidenceFgts: data.get('incidenceFgts') === 'on',
        incidenceIrrf: data.get('incidenceIrrf') === 'on',
        incidenceDsr: data.get('incidenceDsr') === 'on',
        calculationMethod: data.get('calculationMethod'),
        calculationBase: data.get('calculationBase'),
      })
      form?.reset()
      showMessage('Rubrica cadastrada.')
    } catch {
      showMessage('Nao foi possivel cadastrar a rubrica.')
    }
  }

  async function handlePayrollPeriodSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    try {
      await postData('/payroll-periods', {
        companyId: activeCompany.id,
        year: Number(data.get('year')),
        month: Number(data.get('month')),
      })
      form?.reset()
      showMessage('Competencia aberta.')
    } catch {
      showMessage('Nao foi possivel abrir a competencia.')
    }
  }

  async function handlePayrollEntrySubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    const quantity = Number(data.get('quantity') || 0)

    try {
      await postData('/payroll-entries', {
        companyId: activeCompany.id,
        payrollPeriodId: data.get('payrollPeriodId'),
        employeeId: data.get('employeeId'),
        rubricId: data.get('rubricId'),
        entryDate: data.get('entryDate'),
        amount: Number(data.get('amount') || 0),
        quantity: quantity > 0 ? quantity : null,
        reference: data.get('reference'),
        notes: data.get('notes'),
      })
      form?.reset()
      showMessage('Lancamento avulso cadastrado.')
      setMessageModal('Lancamento avulso cadastrado com sucesso.')
    } catch {
      showMessage('Nao foi possivel cadastrar o lancamento avulso.')
    }
  }

  async function handleMassPayrollEntrySubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    const entries = employees
      .filter((employee) => data.get(`include-${employee.id}`) === 'on')
      .map((employee) => {
        const amount = Number(data.get(`amount-${employee.id}`) || 0)
        const quantity = Number(data.get(`quantity-${employee.id}`) || 0)

        return {
          employeeId: employee.id,
          amount,
          quantity: quantity > 0 ? quantity : null,
          notes: data.get(`notes-${employee.id}`),
        }
      })
      .filter((entry) => entry.amount > 0)

    if (entries.length === 0) {
      showMessage('Informe pelo menos um colaborador com valor maior que zero.')
      return
    }

    try {
      await postData('/payroll-entries/mass', {
        companyId: activeCompany.id,
        payrollPeriodId: data.get('payrollPeriodId'),
        rubricId: data.get('rubricId'),
        entryDate: data.get('entryDate'),
        reference: data.get('reference'),
        entries,
      })
      form?.reset()
      const successMessage = `${entries.length} lancamento(s) em massa cadastrado(s).`
      showMessage(successMessage)
      setMessageModal(successMessage)
    } catch {
      showMessage('Nao foi possivel cadastrar os lancamentos em massa.')
    }
  }

  async function handleProductionEntrySubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)

    try {
      await postData('/production-entries', {
        companyId: activeCompany.id,
        payrollPeriodId: data.get('payrollPeriodId'),
        employeeId: data.get('employeeId'),
        productionDate: data.get('productionDate'),
        productionOrderId: data.get('productionOrderId') || null,
        productionOrderProductId: null,
        productionProductId: data.get('productionProductId'),
        productionOperationId: data.get('productionOperationId'),
        productionCellId: data.get('productionCellId') || null,
        quantity: Number(data.get('quantity') || 0),
        notes: data.get('notes'),
      })
      form?.reset()
      showMessage('Apontamento de producao cadastrado.')
      setMessageModal('Apontamento de producao cadastrado com valor calculado pela tabela ativa.')
    } catch (exception) {
      showMessage(exception instanceof Error ? exception.message : 'Nao foi possivel cadastrar o apontamento de producao.')
    }
  }

  async function handleApproveProductionEntry(entry: ProductionEntry) {
    try {
      await postAction(`/production-entries/${entry.id}/approve`)
      showMessage('Apontamento de producao aprovado.')
    } catch (exception) {
      showMessage(exception instanceof Error ? exception.message : 'Nao foi possivel aprovar o apontamento de producao.')
    }
  }

  async function handleCancelProductionEntry(entry: ProductionEntry) {
    const confirmed = window.confirm(`Cancelar o apontamento de producao de ${entry.employeeName}?`)
    if (!confirmed) return

    try {
      await postAction(`/production-entries/${entry.id}/cancel`)
      showMessage('Apontamento de producao cancelado.')
    } catch (exception) {
      showMessage(exception instanceof Error ? exception.message : 'Nao foi possivel cancelar o apontamento de producao.')
    }
  }

  async function handleFixedPayrollEntrySubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    const quantity = Number(data.get('quantity') || 0)
    const endsOn = String(data.get('endsOn') ?? '')

    try {
      await postData('/fixed-payroll-entries', {
        companyId: activeCompany.id,
        employeeId: data.get('employeeId'),
        rubricId: data.get('rubricId'),
        startsOn: data.get('startsOn'),
        endsOn: endsOn ? data.get('endsOn') : null,
        amount: Number(data.get('amount') || 0),
        quantity: quantity > 0 ? quantity : null,
        notes: data.get('notes'),
      })
      form?.reset()
      showMessage('Lancamento fixo cadastrado.')
      setMessageModal('Lancamento fixo cadastrado com sucesso.')
    } catch {
      showMessage('Nao foi possivel cadastrar o lancamento fixo.')
    }
  }

  async function handleFixedPayrollEntryUpdate(entry: FixedPayrollEntry, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const form = event.currentTarget
    const data = new FormData(form)
    const quantity = Number(data.get('quantity') || 0)
    const endsOn = String(data.get('endsOn') ?? '')

    try {
      await postData(`/fixed-payroll-entries/${entry.id}/update`, {
        rubricId: data.get('rubricId'),
        startsOn: data.get('startsOn'),
        endsOn: endsOn ? data.get('endsOn') : null,
        amount: Number(data.get('amount') || 0),
        quantity: quantity > 0 ? quantity : null,
        notes: data.get('notes'),
      })
      showMessage('Lancamento fixo atualizado.')
      setMessageModal('Lancamento fixo atualizado com sucesso.')
    } catch {
      showMessage('Nao foi possivel atualizar o lancamento fixo.')
    }
  }

  async function handleFixedPayrollEntryClose(entry: FixedPayrollEntry, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const data = new FormData(event.currentTarget)

    try {
      await postData(`/fixed-payroll-entries/${entry.id}/close`, {
        endsOn: data.get('endsOn'),
      })
      showMessage('Vigencia do lancamento fixo encerrada.')
      setMessageModal('Vigencia do lancamento fixo encerrada com sucesso.')
    } catch {
      showMessage('Nao foi possivel encerrar o lancamento fixo.')
    }
  }

  async function handleFixedPayrollEntryToggleStatus(entry: FixedPayrollEntry) {
    const action = entry.isActive ? 'deactivate' : 'activate'

    try {
      await postAction(`/fixed-payroll-entries/${entry.id}/${action}`)
      showMessage(entry.isActive ? 'Lancamento fixo inativado.' : 'Lancamento fixo reativado.')
      setMessageModal(entry.isActive ? 'Lancamento fixo inativado com sucesso.' : 'Lancamento fixo reativado com sucesso.')
    } catch {
      showMessage('Nao foi possivel alterar o status do lancamento fixo.')
    }
  }

  async function handleClosePayrollPeriod(period: PayrollPeriod) {
    const confirmed = window.confirm(
      `Fechar a competencia ${period.code}? A competencia precisa estar aprovada e so podera ser alterada apos reabertura.`,
    )

    if (!confirmed) return

    try {
      await postAction(`/payroll-periods/${period.id}/close`)
      showMessage(`Competencia ${period.code} fechada.`)
    } catch {
      showMessage('Nao foi possivel fechar a competencia.')
    }
  }

  async function handleApprovePayrollPeriod(period: PayrollPeriod) {
    const confirmed = window.confirm(
      `Aprovar a folha da competencia ${period.code}? Apos a aprovacao, recalculos e lancamentos ficam bloqueados ate reabertura operacional.`,
    )

    if (!confirmed) return

    try {
      await postAction(`/payroll-periods/${period.id}/approve`)
      showMessage(`Folha da competencia ${period.code} aprovada.`)
    } catch {
      showMessage('Nao foi possivel aprovar a folha da competencia.')
    }
  }

  async function handleReopenPayrollPeriod(period: PayrollPeriod) {
    const confirmed = window.confirm(`Reabrir a competencia ${period.code}?`)

    if (!confirmed) return

    try {
      await postAction(`/payroll-periods/${period.id}/reopen`)
      showMessage(`Competencia ${period.code} reaberta.`)
    } catch {
      showMessage('Nao foi possivel reabrir a competencia.')
    }
  }

  async function handleCalculatePayrollPeriod(period: PayrollPeriod) {
    if (period.status === 'fechada') {
      showMessage('Competencia fechada nao pode ser calculada. Reabra antes de ajustar ou recalcular.')
      return
    }

    if (period.status === 'aprovada') {
      showMessage('Competencia aprovada nao pode ser recalculada. Reabra o processo antes de recalcular.')
      return
    }

    const confirmed = window.confirm(`Calcular a folha da competencia ${period.code}?`)

    if (!confirmed) return

    try {
      await postAction(`/payroll-periods/${period.id}/calculate`)
      showMessage(`Folha da competencia ${period.code} calculada.`)
    } catch {
      showMessage('Nao foi possivel calcular a folha da competencia.')
    }
  }

  async function handleOpenPayrollConference(period: PayrollPeriod) {
    try {
      const response = await apiFetch(`/payroll-calculations/period/${period.id}/current`)

      if (!response.ok) {
        throw new Error('Calculo nao encontrado')
      }

      setPayrollCalculationDetail(await response.json())
      setView('conference')
      showMessage('')
    } catch {
      showMessage('Nao foi possivel carregar a conferencia da folha.')
    }
  }

  async function handleToggleDepartmentStatus(department: Department) {
    const action = department.isActive ? 'deactivate' : 'activate'

    try {
      await postAction(`/departments/${department.id}/${action}`)
      showMessage(department.isActive ? 'Setor inativado.' : 'Setor reativado.')
    } catch {
      showMessage('Nao foi possivel alterar o status do setor.')
    }
  }

  async function handleToggleJobPositionStatus(jobPosition: JobPosition) {
    const action = jobPosition.isActive ? 'deactivate' : 'activate'

    try {
      await postAction(`/job-positions/${jobPosition.id}/${action}`)
      showMessage(jobPosition.isActive ? 'Cargo inativado.' : 'Cargo reativado.')
    } catch {
      showMessage('Nao foi possivel alterar o status do cargo.')
    }
  }

  async function handleToggleRubricStatus(rubric: Rubric) {
    const action = rubric.isActive ? 'deactivate' : 'activate'

    try {
      await postAction(`/rubrics/${rubric.id}/${action}`)
      showMessage(rubric.isActive ? 'Rubrica inativada.' : 'Rubrica reativada.')
    } catch {
      showMessage('Nao foi possivel alterar o status da rubrica.')
    }
  }

  async function handleStatutoryTableSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)
    const ranges = [1, 2, 3, 4, 5]
      .map((index) => {
        const lowerLimit = Number(data.get(`lowerLimit${index}`) || 0)
        const upperLimitValue = String(data.get(`upperLimit${index}`) ?? '')
        const upperLimit = upperLimitValue ? Number(upperLimitValue) : null
        const ratePercent = Number(data.get(`ratePercent${index}`) || 0)
        const deductionAmount = Number(data.get(`deductionAmount${index}`) || 0)

        return { lowerLimit, upperLimit, ratePercent, deductionAmount }
      })
      .filter((range) => range.upperLimit !== null || range.ratePercent > 0 || range.deductionAmount > 0)

    if (ranges.length === 0) {
      showMessage('Informe pelo menos uma faixa para a tabela legal.')
      return
    }

    try {
      await postData('/statutory-tables', {
        companyId: activeCompany.id,
        type: data.get('type'),
        name: data.get('name'),
        startsOn: data.get('startsOn'),
        endsOn: String(data.get('endsOn') ?? '') ? data.get('endsOn') : null,
        notes: data.get('notes'),
        ranges,
      })
      form?.reset()
      showMessage('Tabela legal cadastrada.')
    } catch {
      showMessage('Nao foi possivel cadastrar a tabela legal.')
    }
  }

  async function handleToggleStatutoryTableStatus(table: StatutoryTable) {
    const action = table.isActive ? 'deactivate' : 'activate'

    try {
      await postAction(`/statutory-tables/${table.id}/${action}`)
      showMessage(table.isActive ? 'Tabela legal inativada.' : 'Tabela legal reativada.')
    } catch {
      showMessage('Nao foi possivel alterar o status da tabela legal.')
    }
  }

  async function handleDuplicateStatutoryTable(table: StatutoryTable) {
    const defaultName = `${table.name} - nova vigencia`
    const name = window.prompt('Nome da nova tabela legal', defaultName)
    if (name === null) return

    const startsOn = window.prompt('Inicio da nova vigencia no formato AAAA-MM-DD', nextYearStart(table.startsOn))
    if (!startsOn) return

    const notes = window.prompt('Observacao da nova vigencia', table.notes ?? '') ?? table.notes

    try {
      await postData(`/statutory-tables/${table.id}/duplicate`, {
        name,
        startsOn,
        endsOn: null,
        notes,
      })
      showMessage('Nova vigencia criada a partir da tabela selecionada.')
    } catch {
      showMessage('Nao foi possivel criar a nova vigencia da tabela legal.')
    }
  }

  async function handleProductionRateTableSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return false

    const form = event.currentTarget
    const payload = buildProductionRateTablePayload(form, activeCompany.id)

    if (payload.rates.length === 0) {
      showMessage('Informe pelo menos uma linha de valor para producao.', 'warning')
      return false
    }

    try {
      await postData('/production-rate-tables', payload)
      form.reset()
      showMessage('Tabela de valores de producao cadastrada.')
      setMessageModal('Tabela de valores de producao cadastrada com sucesso.')
      return true
    } catch {
      showMessage('Nao foi possivel cadastrar a tabela de valores de producao.')
      return false
    }
  }

  async function handleProductionRateTableUpdate(table: ProductionRateTable, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return false

    if (table.status !== 'Draft') {
      showMessage('Somente tabelas em rascunho podem ser editadas. Crie uma nova vigencia para alterar valores ativos.', 'warning')
      return false
    }

    const payload = buildProductionRateTablePayload(event.currentTarget, activeCompany.id)
    if (payload.rates.length === 0) {
      showMessage('Informe pelo menos uma linha de valor para producao.', 'warning')
      return false
    }

    try {
      const response = await apiFetch(`/production-rate-tables/${table.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: payload.name,
          effectiveFrom: payload.effectiveFrom,
          effectiveTo: payload.effectiveTo,
          notes: payload.notes,
          rates: payload.rates,
        }),
      })

      if (!response.ok) {
        throw new Error('Nao foi possivel atualizar a tabela')
      }

      await loadData()
      showMessage('Tabela de producao atualizada.')
      return true
    } catch {
      showMessage('Nao foi possivel atualizar a tabela de producao.')
      return false
    }
  }

  async function handleProductionRateTableDuplicate(table: ProductionRateTable, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const data = new FormData(event.currentTarget)

    try {
      const response = await apiFetch(`/production-rate-tables/${table.id}/duplicate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: data.get('name'),
          effectiveFrom: data.get('effectiveFrom'),
          effectiveTo: data.get('effectiveTo') || null,
          notes: data.get('notes') || null,
        }),
      })

      if (!response.ok) {
        throw new Error('Nao foi possivel criar nova vigencia')
      }

      await loadData()
      showMessage('Nova vigencia de producao criada em rascunho.')
      setMessageModal('Nova vigencia criada. Revise os valores e ative quando estiver pronta.')
      return true
    } catch {
      showMessage('Nao foi possivel criar nova vigencia da tabela de producao.')
      return false
    }
  }

  async function handleToggleProductionRateTableStatus(table: ProductionRateTable) {
    const action = table.status === 'Active' ? 'deactivate' : 'activate'

    try {
      await postAction(`/production-rate-tables/${table.id}/${action}`)
      showMessage(table.status === 'Active' ? 'Tabela de producao inativada.' : 'Tabela de producao ativada.')
    } catch {
      showMessage('Nao foi possivel alterar o status da tabela de producao.')
    }
  }

  async function handleUserSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    const data = new FormData(form)

    try {
      await postData('/users', {
        companyId: data.get('companyId') || null,
        fullName: data.get('fullName'),
        email: data.get('email'),
        password: data.get('password'),
        role: data.get('role'),
      })
      form.reset()
      showMessage('Usuario cadastrado.')
      setMessageModal('Usuario cadastrado com sucesso.')
    } catch {
      showMessage('Nao foi possivel cadastrar o usuario.')
    }
  }

  async function handleUserUpdate(user: SystemUser, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const data = new FormData(event.currentTarget)

    try {
      await postData(`/users/${user.id}/update`, {
        companyId: data.get('companyId') || null,
        fullName: data.get('fullName'),
        role: data.get('role'),
      })
      showMessage('Usuario atualizado.')
      setMessageModal('Usuario atualizado com sucesso.')
    } catch {
      showMessage('Nao foi possivel atualizar o usuario.')
    }
  }

  async function handleUserResetPassword(user: SystemUser, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const data = new FormData(event.currentTarget)

    try {
      await postData(`/users/${user.id}/reset-password`, {
        password: data.get('password'),
      })
      showMessage('Senha redefinida.')
      setMessageModal('Senha redefinida com sucesso.')
    } catch {
      showMessage('Nao foi possivel redefinir a senha.')
    }
  }

  async function handleUserToggleStatus(user: SystemUser) {
    const action = user.isActive ? 'deactivate' : 'activate'

    try {
      await postAction(`/users/${user.id}/${action}`)
      showMessage(user.isActive ? 'Usuario inativado.' : 'Usuario reativado.')
      setMessageModal(user.isActive ? 'Usuario inativado com sucesso.' : 'Usuario reativado com sucesso.')
    } catch {
      showMessage('Nao foi possivel alterar o status do usuario.')
    }
  }

  async function handleDapicConfigure(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany) return

    const form = event.currentTarget
    const data = new FormData(form)

    try {
      await postData('/integrations/dapic/configure', {
        companyId: activeCompany.id,
        name: data.get('name'),
        baseUrl: data.get('baseUrl'),
        externalCompanyIdentifier: data.get('externalCompanyIdentifier'),
        integrationToken: data.get('integrationToken'),
      })
      showMessage('Integracao Dapic configurada.')
      setMessageModal('Integracao Dapic configurada com sucesso.')
    } catch {
      showMessage('Nao foi possivel configurar a integracao Dapic.')
    }
  }

  async function handleDapicTest(integration: DapicIntegration) {
    setDapicBusyMessage('Testando conexao com a Dapic...')
    try {
      const response = await apiFetch(`/integrations/dapic/${integration.id}/test-connection`, {
        method: 'POST',
      })

      if (!response.ok) {
        throw new Error('Falha no teste de conexao')
      }

      const result = (await response.json()) as { tokenExpiresAt: string }
      await loadData()
      showMessage('Conexao com Dapic testada.')
      setMessageModal(`Conexao com Dapic validada. Token valido ate ${formatDateTime(result.tokenExpiresAt)}.`)
    } catch {
      await loadData()
      showMessage('Nao foi possivel conectar com a Dapic.')
    } finally {
      setDapicBusyMessage(null)
    }
  }

  async function handleDapicSync(integration: DapicIntegration, resource: string) {
    setDapicBusyMessage(`Importando ${labelDapicResource(resource).toLowerCase()} da Dapic...`)
    try {
      if (resource === 'orders' && !dapicOrderStartDate) {
        showMessage('Informe a data inicial para sincronizar ordens de producao.', 'warning')
        return
      }

      if (resource === 'orders' && !dapicOrderEndDate) {
        showMessage('Informe a data final para sincronizar ordens de producao.', 'warning')
        return
      }

      if (resource === 'orders' && dapicOrderEndDate < dapicOrderStartDate) {
        showMessage('A data final das ordens nao pode ser menor que a data inicial.', 'warning')
        return
      }

      if (resource === 'orders' && dapicOrderEndDate > todayInputDate()) {
        showMessage('A data final das ordens nao pode ser futura.', 'warning')
        return
      }

      const response = await apiFetch(`/integrations/dapic/${integration.id}/sync/${resource}`, {
        method: 'POST',
        headers: resource === 'orders' ? { 'Content-Type': 'application/json' } : undefined,
        body:
          resource === 'orders'
            ? JSON.stringify({
                dataInicial: dapicOrderStartDate,
                dataFinal: dapicOrderEndDate,
              })
            : undefined,
      })

      if (!response.ok) {
        throw new Error('Falha na sincronizacao')
      }

      const result = (await response.json()) as {
        resource: string
        recordsRead: number
        recordsCreated: number
        recordsUpdated: number
        recordsIgnored: number
      }

      await loadData()
      showMessage('Sincronizacao Dapic concluida.')
      setMessageModal(
        `${labelDapicResource(result.resource)} sincronizado: ${result.recordsRead} lido(s), ${result.recordsCreated} criado(s), ${result.recordsUpdated} atualizado(s), ${result.recordsIgnored} ignorado(s).`,
      )
    } catch {
      await loadData()
      showMessage('Nao foi possivel sincronizar dados da Dapic.')
    } finally {
      setDapicBusyMessage(null)
    }
  }

  async function handleDapicEmployeeLink(dapicEmployee: DapicEmployee, employeeId: string) {
    try {
      await postData(`/integrations/dapic/employees/${dapicEmployee.id}/link`, { employeeId })
      showMessage('Funcionario Dapic vinculado ao colaborador.')
      setMessageModal('Funcionario Dapic vinculado ao colaborador com sucesso.')
    } catch {
      showMessage('Nao foi possivel vincular o funcionario Dapic.')
    }
  }

  async function handleDapicEmployeeIgnore(dapicEmployee: DapicEmployee) {
    const reason = window.prompt('Informe o motivo para ignorar este funcionario Dapic:', 'Nao utilizado na folha')
    if (reason === null) return

    try {
      await postData(`/integrations/dapic/employees/${dapicEmployee.id}/ignore`, { reason })
      showMessage('Funcionario Dapic marcado como ignorado.')
    } catch {
      showMessage('Nao foi possivel ignorar o funcionario Dapic.')
    }
  }

  async function handleDapicEmployeeReset(dapicEmployee: DapicEmployee) {
    try {
      await postAction(`/integrations/dapic/employees/${dapicEmployee.id}/reset-link`)
      showMessage('Situacao do funcionario Dapic redefinida.')
    } catch {
      showMessage('Nao foi possivel redefinir o funcionario Dapic.')
    }
  }

  async function handleDapicEmployeeCreateAndLink(dapicEmployee: DapicEmployee, payload: CreateLinkedEmployeePayload) {
    if (!activeCompany) return

    try {
      const createResponse = await apiFetch('/employees', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          companyId: activeCompany.id,
          departmentId: payload.departmentId,
          jobPositionId: payload.jobPositionId,
          registration: payload.registration,
          name: payload.name,
          documentNumber: payload.documentNumber,
          admissionDate: payload.admissionDate,
          compensationModel: payload.compensationModel,
          baseSalary: payload.baseSalary,
          productionUnitValue: payload.productionUnitValue,
          responsibleEmployeeId: null,
          photoUrl: null,
        }),
      })

      if (!createResponse.ok) {
        throw new Error('Nao foi possivel cadastrar o colaborador')
      }

      const employee = (await createResponse.json()) as Employee
      const linkResponse = await apiFetch(`/integrations/dapic/employees/${dapicEmployee.id}/link`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ employeeId: employee.id }),
      })

      if (!linkResponse.ok) {
        throw new Error('Nao foi possivel vincular o colaborador criado')
      }

      await loadData()
      showMessage('Colaborador cadastrado e vinculado ao funcionario Dapic.')
      setMessageModal('Colaborador cadastrado e vinculado ao funcionario Dapic com sucesso.')
    } catch {
      showMessage('Nao foi possivel cadastrar e vincular o funcionario Dapic.')
    }
  }

  if (!session) {
    return <LoginView message={loginMessage} onSubmit={handleLoginSubmit} />
  }

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="brand-kicker">RH Folha</p>
          <h1>Gestao de Folha</h1>
        </div>
        <nav>
          <NavButton active={view === 'dashboard'} onClick={() => setView('dashboard')}>
            Dashboard
          </NavButton>
          <NavButton active={view === 'employees'} onClick={() => setView('employees')}>
            Colaboradores
          </NavButton>
          <NavButton active={view === 'structure'} onClick={() => setView('structure')}>
            Estrutura
          </NavButton>
          <NavButton active={view === 'rubrics'} onClick={() => setView('rubrics')}>
            Rubricas
          </NavButton>
          <NavButton active={view === 'periods'} onClick={() => setView('periods')}>
            Competencias
          </NavButton>
          <NavButton active={view === 'conference'} onClick={() => setView('conference')}>
            Conferencia
          </NavButton>
          <NavButton active={view === 'entries'} onClick={() => setView('entries')}>
            Lancamentos
          </NavButton>
          <NavButton active={view === 'reports'} onClick={() => setView('reports')}>
            Relatorios
          </NavButton>
          {isAdmin(session.user.role) && (
            <NavButton active={view === 'settings'} onClick={() => setView('settings')}>
              Configuracoes
            </NavButton>
          )}
        </nav>
      </aside>

      <section className="content">
        <header className="topbar">
          <div>
            <p className="eyebrow">Ambiente de desenvolvimento</p>
            <h2>{pageTitle(view)}</h2>
            <span>{activeCompany?.tradeName ?? activeCompany?.legalName ?? 'Carregando empresa'}</span>
          </div>
          <div className="topbar-actions">
            <div className="user-chip">
              <strong>{session.user.fullName}</strong>
              <span>{labelUserRole(session.user.role)}</span>
            </div>
            <button type="button" onClick={() => void loadData()}>
              Atualizar
            </button>
            <button type="button" className="secondary-button" onClick={handleLogout}>
              Sair
            </button>
          </div>
        </header>

        {loadState === 'error' && (
          <div className="alert">Nao foi possivel carregar a API. Verifique se o backend esta rodando.</div>
        )}

        {message && <FeedbackBanner message={message} />}

        {messageModal && (
          <MessageModal message={messageModal} onClose={() => setMessageModal('')} />
        )}

        {view === 'dashboard' && (
          <Dashboard
            totals={totals}
            employees={employees}
            departments={departments}
            jobPositions={jobPositions}
            rubrics={rubrics}
            payrollPeriods={payrollPeriods}
            payrollEntries={payrollEntries}
            fixedPayrollEntries={fixedPayrollEntries}
            payrollCalculations={payrollCalculations}
            onNavigate={setView}
          />
        )}

        {view === 'employees' && (
          <EmployeesView
            userRole={session.user.role}
            activeCompany={activeCompany}
            departments={departments}
            jobPositions={jobPositions}
            employees={employees}
            rubrics={rubrics}
            fixedPayrollEntries={fixedPayrollEntries}
            onSubmit={handleEmployeeSubmit}
            onFixedSubmit={handleFixedPayrollEntrySubmit}
            onFixedUpdate={handleFixedPayrollEntryUpdate}
            onFixedClose={handleFixedPayrollEntryClose}
            onFixedToggle={handleFixedPayrollEntryToggleStatus}
            onRefresh={loadData}
          />
        )}

        {view === 'structure' && (
          <StructureView
            canEdit={canOperateHr(session.user.role)}
            activeCompany={activeCompany}
            departments={departments}
            jobPositions={jobPositions}
            onDepartmentSubmit={handleDepartmentSubmit}
            onJobPositionSubmit={handleJobPositionSubmit}
            onToggleDepartmentStatus={handleToggleDepartmentStatus}
            onToggleJobPositionStatus={handleToggleJobPositionStatus}
          />
        )}

        {view === 'rubrics' && (
          <RubricsView
            canEdit={canOperateHr(session.user.role)}
            activeCompany={activeCompany}
            rubrics={rubrics}
            onSubmit={handleRubricSubmit}
            onToggleStatus={handleToggleRubricStatus}
          />
        )}

        {view === 'periods' && (
          <PayrollPeriodsView
            userRole={session.user.role}
            activeCompany={activeCompany}
            payrollPeriods={payrollPeriods}
            payrollCalculations={payrollCalculations}
            onSubmit={handlePayrollPeriodSubmit}
            onCalculate={handleCalculatePayrollPeriod}
            onConference={handleOpenPayrollConference}
            onApprove={handleApprovePayrollPeriod}
            onClose={handleClosePayrollPeriod}
            onReopen={handleReopenPayrollPeriod}
          />
        )}

        {view === 'conference' && (
          <PayrollConferenceView
            calculation={payrollCalculationDetail}
            onBack={() => setView('periods')}
          />
        )}

        {view === 'entries' && (
          <PayrollEntriesView
            canEdit={canOperateHr(session.user.role)}
            activeCompany={activeCompany}
            employees={employees}
            rubrics={rubrics}
            payrollPeriods={payrollPeriods}
            payrollEntries={payrollEntries}
            fixedPayrollEntries={fixedPayrollEntries}
            productionEntries={productionEntries}
            productionCatalogs={productionCatalogs}
            onSubmit={handlePayrollEntrySubmit}
            onMassSubmit={handleMassPayrollEntrySubmit}
            onProductionSubmit={handleProductionEntrySubmit}
            onProductionApprove={handleApproveProductionEntry}
            onProductionCancel={handleCancelProductionEntry}
            onFixedSubmit={handleFixedPayrollEntrySubmit}
            onFixedUpdate={handleFixedPayrollEntryUpdate}
            onFixedClose={handleFixedPayrollEntryClose}
            onFixedToggle={handleFixedPayrollEntryToggleStatus}
          />
        )}

        {view === 'reports' && (
          <ReportsView
            employees={employees}
            payrollCalculations={payrollCalculations}
          />
        )}

        {view === 'settings' && (
          <SettingsView
            activeCompany={activeCompany}
            companies={companies}
            statutoryTables={statutoryTables}
            productionRateTables={productionRateTables}
            users={systemUsers}
            auditLogs={auditLogs}
            hrEmployees={employees}
            departments={departments}
            jobPositions={jobPositions}
            dapicIntegrations={dapicIntegrations}
            dapicLogs={dapicLogs}
            dapicEmployees={dapicEmployees}
            dapicProducts={dapicProducts}
            dapicOperations={dapicOperations}
            dapicCells={dapicCells}
            dapicOrders={dapicOrders}
            dapicBusyMessage={dapicBusyMessage}
            dapicOrderStartDate={dapicOrderStartDate}
            dapicOrderEndDate={dapicOrderEndDate}
            currentUser={session.user}
            onSubmit={handleStatutoryTableSubmit}
            onToggleStatus={handleToggleStatutoryTableStatus}
            onDuplicate={handleDuplicateStatutoryTable}
            onProductionRateTableSubmit={handleProductionRateTableSubmit}
            onProductionRateTableUpdate={handleProductionRateTableUpdate}
            onProductionRateTableDuplicate={handleProductionRateTableDuplicate}
            onProductionRateTableToggleStatus={handleToggleProductionRateTableStatus}
            onUserSubmit={handleUserSubmit}
            onUserUpdate={handleUserUpdate}
            onUserResetPassword={handleUserResetPassword}
            onUserToggleStatus={handleUserToggleStatus}
            onDapicConfigure={handleDapicConfigure}
            onDapicTest={handleDapicTest}
            onDapicSync={handleDapicSync}
            onDapicEmployeeLink={handleDapicEmployeeLink}
            onDapicEmployeeIgnore={handleDapicEmployeeIgnore}
            onDapicEmployeeReset={handleDapicEmployeeReset}
            onDapicEmployeeCreateAndLink={handleDapicEmployeeCreateAndLink}
            onDapicOrderStartDateChange={setDapicOrderStartDate}
            onDapicOrderEndDateChange={setDapicOrderEndDate}
          />
        )}
      </section>
    </main>
  )
}

function Dashboard({
  totals,
  employees,
  departments,
  jobPositions,
  rubrics,
  payrollPeriods,
  payrollEntries,
  fixedPayrollEntries,
  payrollCalculations,
  onNavigate,
}: {
  totals: {
    companies: number
    departments: number
    jobPositions: number
    employees: number
    rubrics: number
    payrollPeriods: number
    payrollEntries: number
    fixedPayrollEntries: number
    calculatedPayrolls: number
    openPayrollPeriods: number
    monthlyPayroll: number
    productionWorkers: number
  }
  employees: Employee[]
  departments: Department[]
  jobPositions: JobPosition[]
  rubrics: Rubric[]
  payrollPeriods: PayrollPeriod[]
  payrollEntries: PayrollEntry[]
  fixedPayrollEntries: FixedPayrollEntry[]
  payrollCalculations: PayrollCalculation[]
  onNavigate: (view: View) => void
}) {
  const monthlyWorkers = employees.length - totals.productionWorkers

  return (
    <>
      <section className="metrics-grid">
        <Metric label="Colaboradores" value={totals.employees} />
        <Metric label="Mensalistas" value={monthlyWorkers} />
        <Metric label="Por producao" value={totals.productionWorkers} />
        <Metric label="Setores" value={totals.departments} />
        <Metric label="Rubricas" value={totals.rubrics} />
        <Metric label="Lancamentos" value={totals.payrollEntries} />
        <Metric label="Fixos" value={totals.fixedPayrollEntries} />
        <Metric label="Folhas calculadas" value={totals.calculatedPayrolls} />
        <Metric label="Competencias abertas" value={totals.openPayrollPeriods} />
      </section>

      <section className="dashboard-grid">
        <Panel title="Pendencias operacionais">
          <ul className="task-list">
            <li>
              <span>{rubrics.length === 0 ? 'Rubricas ainda nao cadastradas' : 'Rubricas base cadastradas'}</span>
              <strong>{rubrics.length === 0 ? 'Pendente' : 'Ok'}</strong>
            </li>
            <li>
              <span>
                {payrollPeriods.some((period) => isPeriodOpen(period.status))
                  ? 'Competencia ativa disponivel'
                  : 'Competencia mensal ainda nao aberta'}
              </span>
              <strong>{payrollPeriods.some((period) => isPeriodOpen(period.status)) ? 'Ok' : 'Pendente'}</strong>
            </li>
            <li>
              <span>
                {fixedPayrollEntries.length > 0 ? 'Lancamentos fixos cadastrados' : 'Lancamentos fixos ainda nao criados'}
              </span>
              <strong>{fixedPayrollEntries.length > 0 ? 'Ok' : 'Pendente'}</strong>
            </li>
            <li>
              <span>
                {payrollEntries.length > 0
                  ? 'Lancamentos avulsos e em massa cadastrados'
                  : 'Lancamentos avulsos ainda nao criados'}
              </span>
              <strong>{payrollEntries.length > 0 ? 'Ok' : 'Pendente'}</strong>
            </li>
            <li>
              <span>
                {payrollCalculations.length > 0 ? 'Folha calculada disponivel' : 'Nenhuma folha calculada'}
              </span>
              <strong>{payrollCalculations.length > 0 ? 'Ok' : 'Pendente'}</strong>
            </li>
          </ul>
        </Panel>

        <Panel title="Acoes rapidas">
          <div className="quick-actions">
            <button type="button" onClick={() => onNavigate('employees')}>
              Cadastrar colaborador
            </button>
            <button type="button" onClick={() => onNavigate('structure')}>
              Gerenciar estrutura
            </button>
            <button type="button" onClick={() => onNavigate('rubrics')}>
              Gerenciar rubricas
            </button>
            <button type="button" onClick={() => onNavigate('periods')}>
              Abrir competencia
            </button>
            <button type="button" onClick={() => onNavigate('entries')}>
              Fazer lancamento
            </button>
          </div>
        </Panel>
      </section>

      <section className="tables-grid">
        <DataTable
          title="Colaboradores recentes"
          columns={['Matricula', 'Nome', 'Modelo', 'Base']}
          rows={employees.slice(0, 5).map((employee) => [
            employee.registration,
            employee.name,
            labelCompensation(employee.compensationModel),
            employee.compensationModel === 'producao'
              ? formatCurrency(employee.productionUnitValue)
              : formatCurrency(employee.baseSalary),
          ])}
        />
        <DataTable
          title="Estrutura cadastrada"
          columns={['Tipo', 'Quantidade']}
          rows={[
            ['Setores', String(departments.length)],
            ['Cargos', String(jobPositions.length)],
            ['Rubricas', String(rubrics.length)],
            ['Competencias', String(payrollPeriods.length)],
          ]}
        />
      </section>
    </>
  )
}

function PayrollEntriesView({
  canEdit,
  activeCompany,
  employees,
  rubrics,
  payrollPeriods,
  payrollEntries,
  fixedPayrollEntries,
  productionEntries,
  productionCatalogs,
  onSubmit,
  onMassSubmit,
  onProductionSubmit,
  onProductionApprove,
  onProductionCancel,
  onFixedSubmit,
  onFixedUpdate,
  onFixedClose,
  onFixedToggle,
}: {
  canEdit: boolean
  activeCompany: Company | undefined
  employees: Employee[]
  rubrics: Rubric[]
  payrollPeriods: PayrollPeriod[]
  payrollEntries: PayrollEntry[]
  fixedPayrollEntries: FixedPayrollEntry[]
  productionEntries: ProductionEntry[]
  productionCatalogs: ProductionCatalogs
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onMassSubmit: (event: FormEvent<HTMLFormElement>) => void
  onProductionSubmit: (event: FormEvent<HTMLFormElement>) => void
  onProductionApprove: (entry: ProductionEntry) => void
  onProductionCancel: (entry: ProductionEntry) => void
  onFixedSubmit: (event: FormEvent<HTMLFormElement>) => void
  onFixedUpdate: (entry: FixedPayrollEntry, event: FormEvent<HTMLFormElement>) => void
  onFixedClose: (entry: FixedPayrollEntry, event: FormEvent<HTMLFormElement>) => void
  onFixedToggle: (entry: FixedPayrollEntry) => void
}) {
  const openPeriods = payrollPeriods.filter((period) => isPeriodOpen(period.status))
  const manualRubrics = rubrics.filter((rubric) => rubric.allowsManualEntry && rubric.isActive)
  const massRubrics = manualRubrics.filter((rubric) => rubric.allowsMassEntry)
  const fixedRubrics = manualRubrics.filter((rubric) => rubric.allowsFixedEntry)
  const defaultPeriod = openPeriods[0]
  const defaultManualPeriodId = selectDefaultValue(openPeriods)
  const defaultManualEmployeeId = selectDefaultValue(employees)
  const defaultManualRubricId = selectDefaultValue(manualRubrics)
  const defaultMassPeriodId = selectDefaultValue(openPeriods)
  const defaultMassRubricId = selectDefaultValue(massRubrics)
  const defaultProductionPeriodId = selectDefaultValue(openPeriods)
  const defaultProductionEmployeeId = selectDefaultValue(employees.filter((employee) => employee.compensationModel === 'producao'))
  const defaultProductionProductId = selectDefaultValue(productionCatalogs.products)
  const defaultProductionOperationId = selectDefaultValue(productionCatalogs.operations)
  const defaultProductionCellId = selectDefaultValue(productionCatalogs.cells)
  const defaultProductionOrderId = selectDefaultValue(productionCatalogs.orders)
  const productionEmployees = employees.filter((employee) => employee.compensationModel === 'producao')
  const [massDepartmentId, setMassDepartmentId] = useState('todos')
  const [massJobPositionId, setMassJobPositionId] = useState('todos')
  const [massCalculationMode, setMassCalculationMode] = useState('valor_fixo')
  const [massDefaultValue, setMassDefaultValue] = useState('')
  const [massDefaultQuantity, setMassDefaultQuantity] = useState('')
  const [massDefaultNote, setMassDefaultNote] = useState('')
  const [massSelected, setMassSelected] = useState<Record<string, boolean>>({})
  const [massAmounts, setMassAmounts] = useState<Record<string, string>>({})
  const [massQuantities, setMassQuantities] = useState<Record<string, string>>({})
  const [massNotes, setMassNotes] = useState<Record<string, string>>({})
  const [entryModal, setEntryModal] = useState<'manual' | 'mass' | 'production' | 'fixed' | null>(null)
  const [editingFixedEntry, setEditingFixedEntry] = useState<FixedPayrollEntry | null>(null)
  const [closingFixedEntry, setClosingFixedEntry] = useState<FixedPayrollEntry | null>(null)
  const departmentOptions = Array.from(
    new Map(employees.map((employee) => [employee.departmentId, employee.departmentName])).entries(),
  )
  const jobPositionOptions = Array.from(
    new Map(employees.map((employee) => [employee.jobPositionId, employee.jobPositionName])).entries(),
  )
  const filteredEmployees = employees.filter(
    (employee) =>
      (massDepartmentId === 'todos' || employee.departmentId === massDepartmentId) &&
      (massJobPositionId === 'todos' || employee.jobPositionId === massJobPositionId),
  )
  const allFilteredSelected =
    filteredEmployees.length > 0 && filteredEmployees.every((employee) => massSelected[employee.id])

  function toggleFilteredEmployees() {
    const shouldSelect = !allFilteredSelected

    setMassSelected((current) => {
      const next = { ...current }
      filteredEmployees.forEach((employee) => {
        next[employee.id] = shouldSelect
      })

      return next
    })
  }

  function applyDefaultsToFilteredEmployees() {
    const defaultValue = Number(massDefaultValue || 0)
    const hasDefaultValue = defaultValue > 0
    const hasDefaultQuantity = massDefaultQuantity.trim().length > 0
    const hasDefaultNote = massDefaultNote.trim().length > 0
    if (!hasDefaultValue && !hasDefaultQuantity && !hasDefaultNote) return

    if (hasDefaultValue) {
      setMassAmounts((current) => {
        const next = { ...current }
        filteredEmployees.forEach((employee) => {
          const amount =
            massCalculationMode === 'percentual'
              ? roundCurrency(employee.baseSalary * (defaultValue / 100))
              : defaultValue

          next[employee.id] = amount > 0 ? String(amount) : ''
        })

        return next
      })
    }

    if (hasDefaultQuantity) {
      setMassQuantities((current) => {
        const next = { ...current }
        filteredEmployees.forEach((employee) => {
          next[employee.id] = massDefaultQuantity
        })

        return next
      })
    }

    if (hasDefaultNote) {
      setMassNotes((current) => {
        const next = { ...current }
        filteredEmployees.forEach((employee) => {
          next[employee.id] = massDefaultNote
        })

        return next
      })
    }

    setMassSelected((current) => {
      const next = { ...current }
      filteredEmployees.forEach((employee) => {
        next[employee.id] = true
      })

      return next
    })
  }

  return (
    <>
      {canEdit && (
        <section className="section-actions">
          <button type="button" onClick={() => setEntryModal('manual')}>
            Novo lancamento avulso
          </button>
          <button type="button" onClick={() => setEntryModal('mass')}>
            Lancamento em massa
          </button>
          <button type="button" onClick={() => setEntryModal('production')}>
            Apontamento de producao
          </button>
          <button type="button" onClick={() => setEntryModal('fixed')}>
            Lancamento fixo
          </button>
        </section>
      )}

      {entryModal === 'manual' && (
        <Modal title="Novo lancamento avulso" onClose={() => setEntryModal(null)} size="wide">
        <form className="entry-form" onSubmit={(event) => void onSubmit(event)}>
          <label>
            Competencia
            <select name="payrollPeriodId" defaultValue={defaultManualPeriodId} required>
              {openPeriods.length !== 1 && (
                <option value="" disabled>
                  Selecione a competencia
                </option>
              )}
              {openPeriods.map((period) => (
                <option key={period.id} value={period.id}>
                  {period.code} - {labelPeriodStatus(period.status)}
                </option>
              ))}
            </select>
          </label>
          <label>
            Colaborador
            <select name="employeeId" defaultValue={defaultManualEmployeeId} required>
              {employees.length !== 1 && (
                <option value="" disabled>
                  Selecione o colaborador
                </option>
              )}
              {employees.map((employee) => (
                <option key={employee.id} value={employee.id}>
                  {employee.registration} - {employee.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Rubrica
            <select name="rubricId" defaultValue={defaultManualRubricId} required>
              {manualRubrics.length !== 1 && (
                <option value="" disabled>
                  Selecione a rubrica
                </option>
              )}
              {manualRubrics.map((rubric) => (
                <option key={rubric.id} value={rubric.id}>
                  {rubric.code} - {rubric.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Data
            <input name="entryDate" type="date" defaultValue={defaultPeriod?.startsOn ?? ''} required />
          </label>
          <label>
            Valor
            <input name="amount" type="number" min="0.01" step="0.01" required />
          </label>
          <label>
            Quantidade
            <input name="quantity" type="number" min="0" step="0.0001" placeholder="Opcional" />
          </label>
          <label>
            Referencia
            <input name="reference" placeholder="Ex: ADV-06/2026" />
          </label>
          <label className="span-2">
            Observacao
            <input name="notes" placeholder="Detalhe operacional opcional" />
          </label>
          <div className="modal-actions">
            <button type="button" onClick={() => setEntryModal(null)}>
              Cancelar
            </button>
            <button
              type="submit"
              disabled={!activeCompany || openPeriods.length === 0 || employees.length === 0 || manualRubrics.length === 0}
            >
              Salvar lancamento
            </button>
          </div>
        </form>
        </Modal>
      )}

      {entryModal === 'mass' && (
        <Modal title="Lancamento em massa" onClose={() => setEntryModal(null)} size="wide">
        <form className="mass-entry-form" onSubmit={(event) => void onMassSubmit(event)}>
          <div className="entry-form">
            <label>
              Competencia
              <select name="payrollPeriodId" defaultValue={defaultMassPeriodId} required>
                {openPeriods.length !== 1 && (
                  <option value="" disabled>
                    Selecione a competencia
                  </option>
                )}
                {openPeriods.map((period) => (
                  <option key={period.id} value={period.id}>
                    {period.code} - {labelPeriodStatus(period.status)}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Rubrica
              <select name="rubricId" defaultValue={defaultMassRubricId} required>
                {massRubrics.length !== 1 && (
                  <option value="" disabled>
                    Selecione a rubrica
                  </option>
                )}
                {massRubrics.map((rubric) => (
                  <option key={rubric.id} value={rubric.id}>
                    {rubric.code} - {rubric.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Data
              <input name="entryDate" type="date" defaultValue={defaultPeriod?.startsOn ?? ''} required />
            </label>
            <label>
              Referencia
              <input name="reference" placeholder="Ex: LOTE-06/2026" />
            </label>
            <label>
              Setor
              <select value={massDepartmentId} onChange={(event) => setMassDepartmentId(event.target.value)}>
                <option value="todos">Todos</option>
                {departmentOptions.map(([departmentId, departmentName]) => (
                  <option key={departmentId} value={departmentId}>
                    {departmentName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Cargo
              <select value={massJobPositionId} onChange={(event) => setMassJobPositionId(event.target.value)}>
                <option value="todos">Todos</option>
                {jobPositionOptions.map(([jobPositionId, jobPositionName]) => (
                  <option key={jobPositionId} value={jobPositionId}>
                    {jobPositionName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Calculo
              <select value={massCalculationMode} onChange={(event) => setMassCalculationMode(event.target.value)}>
                <option value="valor_fixo">Valor fixo</option>
                <option value="percentual">Percentual do salario</option>
              </select>
            </label>
            <label>
              Valor padrao
              <input
                type="number"
                min="0"
                step="0.01"
                value={massDefaultValue}
                onChange={(event) => setMassDefaultValue(event.target.value)}
                placeholder={massCalculationMode === 'percentual' ? 'Ex: 6' : 'Ex: 150.00'}
              />
            </label>
            <label>
              Quantidade padrao
              <input
                type="number"
                min="0"
                step="0.0001"
                value={massDefaultQuantity}
                onChange={(event) => setMassDefaultQuantity(event.target.value)}
                placeholder="Opcional"
              />
            </label>
            <label>
              Observacao padrao
              <input
                value={massDefaultNote}
                onChange={(event) => setMassDefaultNote(event.target.value)}
                placeholder="Opcional"
              />
            </label>
            <button type="button" onClick={applyDefaultsToFilteredEmployees} disabled={filteredEmployees.length === 0}>
              Aplicar padroes
            </button>
            <div className="modal-actions compact-actions">
              <button type="button" onClick={() => setEntryModal(null)}>
                Cancelar
              </button>
              <button
                type="submit"
                disabled={!activeCompany || openPeriods.length === 0 || employees.length === 0 || massRubrics.length === 0}
              >
                Salvar lote
              </button>
            </div>
          </div>

          <div className="table-scroll mass-entry-table">
            <table>
              <thead>
                <tr>
                  <th>
                    <button type="button" className="table-action" onClick={toggleFilteredEmployees}>
                      {allFilteredSelected ? 'Limpar' : 'Todos'}
                    </button>
                  </th>
                  <th>Colaborador</th>
                  <th>Setor</th>
                  <th>Cargo</th>
                  <th>Modelo</th>
                  <th>Valor</th>
                  <th>Quantidade</th>
                  <th>Observacao</th>
                </tr>
              </thead>
              <tbody>
                {filteredEmployees.map((employee) => (
                  <tr key={employee.id}>
                    <td>
                      <input
                        name={`include-${employee.id}`}
                        type="checkbox"
                        checked={Boolean(massSelected[employee.id])}
                        onChange={(event) =>
                          setMassSelected((current) => ({ ...current, [employee.id]: event.target.checked }))
                        }
                      />
                    </td>
                    <td>{employee.registration} - {employee.name}</td>
                    <td>{employee.departmentName}</td>
                    <td>{employee.jobPositionName}</td>
                    <td>{labelCompensation(employee.compensationModel)}</td>
                    <td>
                      <input
                        name={`amount-${employee.id}`}
                        type="number"
                        min="0"
                        step="0.01"
                        value={massAmounts[employee.id] ?? ''}
                        onChange={(event) =>
                          setMassAmounts((current) => ({ ...current, [employee.id]: event.target.value }))
                        }
                      />
                    </td>
                    <td>
                      <input
                        name={`quantity-${employee.id}`}
                        type="number"
                        min="0"
                        step="0.0001"
                        value={massQuantities[employee.id] ?? ''}
                        onChange={(event) =>
                          setMassQuantities((current) => ({ ...current, [employee.id]: event.target.value }))
                        }
                      />
                    </td>
                    <td>
                      <input
                        name={`notes-${employee.id}`}
                        placeholder="Opcional"
                        value={massNotes[employee.id] ?? ''}
                        onChange={(event) =>
                          setMassNotes((current) => ({ ...current, [employee.id]: event.target.value }))
                        }
                      />
                    </td>
                  </tr>
                ))}
                {filteredEmployees.length === 0 && (
                  <tr>
                    <td colSpan={8}>Nenhum colaborador encontrado para os filtros selecionados.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </form>
        </Modal>
      )}

      {entryModal === 'production' && (
        <Modal title="Apontamento de producao" onClose={() => setEntryModal(null)} size="wide">
        <form className="entry-form" onSubmit={(event) => void onProductionSubmit(event)}>
          <label>
            Competencia
            <select name="payrollPeriodId" defaultValue={defaultProductionPeriodId} required>
              {openPeriods.length !== 1 && (
                <option value="" disabled>
                  Selecione a competencia
                </option>
              )}
              {openPeriods.map((period) => (
                <option key={period.id} value={period.id}>
                  {period.code} - {labelPeriodStatus(period.status)}
                </option>
              ))}
            </select>
          </label>
          <SearchableSelect
            name="employeeId"
            label="Colaborador"
            defaultValue={defaultProductionEmployeeId}
            placeholder="Selecione o colaborador"
            options={productionEmployees.map((employee) => ({
              value: employee.id,
              label: `${employee.registration} - ${employee.name}`,
              description: `${employee.departmentName} / ${employee.jobPositionName}`,
            }))}
          />
          <label>
            Data
            <input name="productionDate" type="date" defaultValue={defaultPeriod?.startsOn ?? ''} required />
          </label>
          <SearchableSelect
            name="productionProductId"
            label="Produto"
            defaultValue={defaultProductionProductId}
            placeholder="Selecione o produto"
            options={productionCatalogs.products.map((product) => ({
              value: product.id,
              label: `${product.reference} - ${product.factoryDescription}`,
              description: labelDapicStatus(product.status),
            }))}
          />
          <SearchableSelect
            name="productionOperationId"
            label="Operacao"
            defaultValue={defaultProductionOperationId}
            placeholder="Selecione a operacao"
            options={productionCatalogs.operations.map((operation) => ({
              value: operation.id,
              label: operation.name,
              description: operation.description ?? labelDapicStatus(operation.status),
            }))}
          />
          <SearchableSelect
            name="productionCellId"
            label="Celula"
            defaultValue={defaultProductionCellId}
            placeholder="Sem celula"
            allowEmpty
            options={productionCatalogs.cells.map((cell) => ({
              value: cell.id,
              label: cell.name,
              description: cell.description ?? labelDapicStatus(cell.status),
            }))}
          />
          <SearchableSelect
            name="productionOrderId"
            label="Ordem"
            defaultValue={defaultProductionOrderId}
            placeholder="Sem ordem"
            allowEmpty
            options={productionCatalogs.orders.map((order) => ({
              value: order.id,
              label: order.number ?? order.description ?? 'Ordem sem numero',
              description: [order.issueDate ? formatDate(order.issueDate) : null, labelDapicStatus(order.status)]
                .filter(Boolean)
                .join(' / '),
            }))}
          />
          <label>
            Quantidade
            <input name="quantity" type="number" min="0.0001" step="0.0001" required />
          </label>
          <label className="span-2">
            Observacao
            <input name="notes" placeholder="Detalhe operacional opcional" />
          </label>
          <div className="modal-actions">
            <button type="button" onClick={() => setEntryModal(null)}>
              Cancelar
            </button>
            <button
              type="submit"
              disabled={
                !activeCompany ||
                openPeriods.length === 0 ||
                productionEmployees.length === 0 ||
                productionCatalogs.products.length === 0 ||
                productionCatalogs.operations.length === 0
              }
            >
              Salvar apontamento
            </button>
          </div>
        </form>
        </Modal>
      )}

      {entryModal === 'fixed' && (
        <Modal title="Lancamento fixo por colaborador" onClose={() => setEntryModal(null)} size="wide">
        <form className="entry-form" onSubmit={(event) => void onFixedSubmit(event)}>
          <label>
            Colaborador
            <select name="employeeId" defaultValue="" required>
              <option value="" disabled>
                Selecione o colaborador
              </option>
              {employees.map((employee) => (
                <option key={employee.id} value={employee.id}>
                  {employee.registration} - {employee.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Rubrica
            <select name="rubricId" defaultValue="" required>
              <option value="" disabled>
                Selecione a rubrica fixa
              </option>
              {fixedRubrics.map((rubric) => (
                <option key={rubric.id} value={rubric.id}>
                  {rubric.code} - {rubric.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Inicio vigencia
            <input name="startsOn" type="date" defaultValue={defaultPeriod?.startsOn ?? ''} required />
          </label>
          <label>
            Fim vigencia
            <input name="endsOn" type="date" />
          </label>
          <label>
            Valor
            <input name="amount" type="number" min="0.01" step="0.01" required />
          </label>
          <label>
            Quantidade
            <input name="quantity" type="number" min="0" step="0.0001" placeholder="Opcional" />
          </label>
          <label className="span-2">
            Observacao
            <input name="notes" placeholder="Ex: desconto mensal recorrente" />
          </label>
          <div className="modal-actions">
            <button type="button" onClick={() => setEntryModal(null)}>
              Cancelar
            </button>
            <button type="submit" disabled={!activeCompany || employees.length === 0 || fixedRubrics.length === 0}>
              Salvar fixo
            </button>
          </div>
        </form>
        </Modal>
      )}

      {editingFixedEntry && (
        <Modal title="Editar lancamento fixo" onClose={() => setEditingFixedEntry(null)}>
          <FixedPayrollEntryEditForm
            entry={editingFixedEntry}
            fixedRubrics={fixedRubrics}
            onCancel={() => setEditingFixedEntry(null)}
            onSubmit={async (event) => {
              await Promise.resolve(onFixedUpdate(editingFixedEntry, event))
              setEditingFixedEntry(null)
            }}
          />
        </Modal>
      )}

      {closingFixedEntry && (
        <Modal title="Encerrar lancamento fixo" onClose={() => setClosingFixedEntry(null)}>
          <FixedPayrollEntryCloseForm
            entry={closingFixedEntry}
            onCancel={() => setClosingFixedEntry(null)}
            onSubmit={async (event) => {
              await Promise.resolve(onFixedClose(closingFixedEntry, event))
              setClosingFixedEntry(null)
            }}
          />
        </Modal>
      )}

      <DataTable
        title="Lancamentos fixos"
        columns={canEdit ? ['Colaborador', 'Rubrica', 'Tipo', 'Valor', 'Quantidade', 'Vigencia', 'Status', 'Acoes'] : ['Colaborador', 'Rubrica', 'Tipo', 'Valor', 'Quantidade', 'Vigencia', 'Status']}
        rows={fixedPayrollEntries.map((entry) => [
          `${entry.employeeRegistration} - ${entry.employeeName}`,
          `${entry.rubricCode} - ${entry.rubricName}`,
          labelRubricType(entry.rubricType),
          formatCurrency(entry.amount),
          entry.quantity ? formatNumber(entry.quantity) : '-',
          `${formatDate(entry.startsOn)} a ${entry.endsOn ? formatDate(entry.endsOn) : 'sem fim'}`,
          activeBadge(entry.isActive),
          ...(canEdit ? [fixedPayrollEntryActions(entry, setEditingFixedEntry, setClosingFixedEntry, onFixedToggle)] : []),
        ])}
      />

      <DataTable
        title="Apontamentos de producao"
        columns={canEdit ? ['Competencia', 'Colaborador', 'Produto', 'Operacao', 'Celula', 'Quantidade', 'Valor unitario', 'Total', 'Data', 'Status', 'Acoes'] : ['Competencia', 'Colaborador', 'Produto', 'Operacao', 'Celula', 'Quantidade', 'Valor unitario', 'Total', 'Data', 'Status']}
        pageSize={15}
        rows={productionEntries.map((entry) => [
          entry.payrollPeriodCode,
          `${entry.employeeRegistration} - ${entry.employeeName}`,
          `${entry.productReference} - ${entry.productDescription}`,
          entry.operationName,
          entry.cellName ?? '-',
          formatNumber(entry.quantity),
          formatCurrency(entry.unitValue),
          formatCurrency(entry.totalAmount),
          formatDate(entry.productionDate),
          productionEntryStatusBadge(entry.status),
          ...(canEdit
            ? [
                <div className="table-actions">
                  {actionButton('Aprovar', () => onProductionApprove(entry), entry.status !== 'Draft')}
                  {actionButton('Cancelar', () => onProductionCancel(entry), entry.status === 'IntegratedIntoPayroll' || entry.status === 'Canceled')}
                </div>,
              ]
            : []),
        ])}
      />

      <DataTable
        title="Lancamentos avulsos"
        columns={['Competencia', 'Colaborador', 'Rubrica', 'Tipo', 'Origem', 'Valor', 'Quantidade', 'Data', 'Status']}
        rows={payrollEntries.map((entry) => [
          entry.payrollPeriodCode,
          `${entry.employeeRegistration} - ${entry.employeeName}`,
          `${entry.rubricCode} - ${entry.rubricName}`,
          labelRubricType(entry.rubricType),
          labelEntryOrigin(entry.origin),
          formatCurrency(entry.amount),
          entry.quantity ? formatNumber(entry.quantity) : '-',
          formatDate(entry.entryDate),
          entryStatusBadge(entry.status),
        ])}
      />
    </>
  )
}

function PayrollPeriodsView({
  userRole,
  activeCompany,
  payrollPeriods,
  payrollCalculations,
  onSubmit,
  onCalculate,
  onConference,
  onApprove,
  onClose,
  onReopen,
}: {
  userRole: string
  activeCompany: Company | undefined
  payrollPeriods: PayrollPeriod[]
  payrollCalculations: PayrollCalculation[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onCalculate: (period: PayrollPeriod) => void
  onConference: (period: PayrollPeriod) => void
  onApprove: (period: PayrollPeriod) => void
  onClose: (period: PayrollPeriod) => void
  onReopen: (period: PayrollPeriod) => void
}) {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const canOpen = canOperateHr(userRole)
  const canCalculate = canWorkPayroll(userRole)
  const canApprove = canApprovePayroll(userRole)
  const canCloseOrReopen = isAdmin(userRole)
  const calculationsByPeriod = new Map(
    payrollCalculations.map((calculation) => [calculation.payrollPeriodId, calculation]),
  )

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    await Promise.resolve(onSubmit(event))
    setIsCreateModalOpen(false)
  }

  return (
    <>
      {canOpen && (
        <section className="section-actions">
          <button type="button" onClick={() => setIsCreateModalOpen(true)}>
            Abrir competencia
          </button>
        </section>
      )}

      {isCreateModalOpen && (
        <Modal title="Abrir competencia" onClose={() => setIsCreateModalOpen(false)}>
        <form className="period-form" onSubmit={(event) => void handleSubmit(event)}>
          <label>
            Ano
            <input name="year" type="number" min="2020" max="2100" defaultValue="2026" required />
          </label>
          <label>
            Mes
            <select name="month" defaultValue="6" required>
              {Array.from({ length: 12 }, (_, index) => index + 1).map((month) => (
                <option key={month} value={month}>
                  {String(month).padStart(2, '0')}
                </option>
              ))}
            </select>
          </label>
          <div className="modal-actions">
            <button type="button" onClick={() => setIsCreateModalOpen(false)}>
              Cancelar
            </button>
            <button type="submit" disabled={!activeCompany}>
              Abrir competencia
            </button>
          </div>
        </form>
        </Modal>
      )}

      <DataTable
        title="Competencias"
        columns={['Competencia', 'Periodo', 'Status', 'Liquido', 'Abertura', 'Fechamento', 'Acoes']}
        rows={payrollPeriods.map((period) => [
          period.code,
          `${formatDate(period.startsOn)} a ${formatDate(period.endsOn)}`,
          statusBadge(period.status),
          calculationsByPeriod.has(period.id)
            ? formatCurrency(calculationsByPeriod.get(period.id)!.totalLiquido)
            : '-',
          formatDateTime(period.openedAt),
          period.closedAt ? formatDateTime(period.closedAt) : '-',
          periodActions(
            period,
            calculationsByPeriod.has(period.id),
            onCalculate,
            onConference,
            onApprove,
            onClose,
            onReopen,
            canCalculate,
            canApprove,
            canCloseOrReopen,
          ),
        ])}
      />

      <DataTable
        title="Resumo das folhas calculadas"
        columns={['Competencia', 'Calculo', 'Colaboradores', 'Proventos', 'Descontos', 'Liquido']}
        rows={payrollCalculations.map((calculation) => [
          calculation.periodCode,
          formatDateTime(calculation.calculatedAt),
          String(calculation.employeeCount),
          formatCurrency(calculation.totalProventos),
          formatCurrency(calculation.totalDescontos),
          formatCurrency(calculation.totalLiquido),
        ])}
      />
    </>
  )
}

function PayrollConferenceView({
  calculation,
  onBack,
}: {
  calculation: PayrollCalculationDetail | null
  onBack: () => void
}) {
  const [selectedEmployeeId, setSelectedEmployeeId] = useState('')

  if (!calculation) {
    return (
      <Panel title="Conferencia da folha">
        <div className="empty-state">
          <span>Nenhuma folha selecionada para conferencia.</span>
          <button type="button" onClick={onBack}>
            Voltar para competencias
          </button>
        </div>
      </Panel>
    )
  }

  const activeCalculation = calculation
  const employeeSummaries = summarizePayrollByEmployee(activeCalculation.items)

  const totalFgts = employeeSummaries.reduce((total, summary) => total + summary.fgts, 0)
  const totalInss = employeeSummaries.reduce((total, summary) => total + summary.inss, 0)
  const totalIrrf = employeeSummaries.reduce((total, summary) => total + summary.irrf, 0)
  const selectedSummary =
    employeeSummaries.find((summary) => summary.employeeId === selectedEmployeeId) ?? employeeSummaries[0]
  const selectedItems = activeCalculation.items.filter((item) => item.employeeId === selectedSummary?.employeeId)
  const proventoItems = selectedItems.filter((item) => item.rubricType === 'provento')
  const descontoItems = selectedItems.filter((item) => item.rubricType === 'desconto')
  const informativaItems = selectedItems.filter((item) => item.rubricType === 'informativa')

  function printSelectedPayslip() {
    if (!selectedSummary) return

    openPayslipPrintWindow(activeCalculation.periodCode, selectedSummary, proventoItems, descontoItems, informativaItems)
  }

  function printAllPayslips() {
    openAllPayslipsPrintWindow(activeCalculation.periodCode, employeeSummaries, activeCalculation.items)
  }

  return (
    <>
      <section className="conference-header">
        <div>
          <p className="eyebrow">Folha calculada</p>
          <h3>{calculation.periodCode}</h3>
          <span>Calculada em {formatDateTime(calculation.calculatedAt)}</span>
        </div>
        <button type="button" onClick={onBack}>
          Voltar
        </button>
      </section>

      <section className="metrics-grid">
        <Metric label="Colaboradores" value={calculation.employeeCount} />
        <Metric label="Proventos" value={formatCurrency(calculation.totalProventos)} />
        <Metric label="Descontos" value={formatCurrency(calculation.totalDescontos)} />
        <Metric label="Liquido" value={formatCurrency(calculation.totalLiquido)} />
        <Metric label="FGTS informativo" value={formatCurrency(totalFgts)} />
        <Metric label="INSS" value={formatCurrency(totalInss)} />
        <Metric label="IRRF" value={formatCurrency(totalIrrf)} />
      </section>

      <DataTable
        title="Conferencia por colaborador"
        columns={[
          'Colaborador',
          'Base FGTS',
          'FGTS',
          'Base INSS',
          'INSS',
          'Base IRRF',
          'IRRF',
          'Proventos',
          'Descontos',
          'Liquido',
        ]}
        rows={employeeSummaries.map((summary) => [
          summary.employee,
          formatCurrency(summary.baseFgts),
          formatCurrency(summary.fgts),
          formatCurrency(summary.baseInss),
          formatCurrency(summary.inss),
          formatCurrency(summary.baseIrrf),
          formatCurrency(summary.irrf),
          formatCurrency(summary.proventos),
          formatCurrency(summary.descontos),
          formatCurrency(summary.liquido),
        ])}
      />

      {selectedSummary && (
        <Panel title="Holerite por colaborador">
          <div className="payslip-toolbar">
            <label>
              Colaborador
              <select value={selectedSummary.employeeId} onChange={(event) => setSelectedEmployeeId(event.target.value)}>
                {employeeSummaries.map((summary) => (
                  <option key={summary.employeeId} value={summary.employeeId}>
                    {summary.employee}
                  </option>
                ))}
              </select>
            </label>
            <button type="button" onClick={printSelectedPayslip}>
              Imprimir holerite
            </button>
            <button type="button" onClick={printAllPayslips}>
              Imprimir todos
            </button>
          </div>

          <section className="payslip">
            <div className="payslip-header">
              <div>
                <p className="eyebrow">Competencia</p>
                <h3>{calculation.periodCode}</h3>
              </div>
              <div>
                <span>{selectedSummary.registration}</span>
                <strong>{selectedSummary.name}</strong>
              </div>
            </div>

            <section className="payslip-bases">
              <Metric label="Base FGTS" value={formatCurrency(selectedSummary.baseFgts)} />
              <Metric label="FGTS" value={formatCurrency(selectedSummary.fgts)} />
              <Metric label="Base INSS" value={formatCurrency(selectedSummary.baseInss)} />
              <Metric label="INSS" value={formatCurrency(selectedSummary.inss)} />
              <Metric label="Base IRRF" value={formatCurrency(selectedSummary.baseIrrf)} />
              <Metric label="IRRF" value={formatCurrency(selectedSummary.irrf)} />
            </section>

            <section className="payslip-columns">
              <PayslipList title="Proventos" items={proventoItems} />
              <PayslipList title="Descontos" items={descontoItems} />
              <PayslipList title="Informativas" items={informativaItems} />
            </section>

            <section className="payslip-total">
              <span>Total de proventos: {formatCurrency(selectedSummary.proventos)}</span>
              <span>Total de descontos: {formatCurrency(selectedSummary.descontos)}</span>
              <strong>Liquido: {formatCurrency(selectedSummary.liquido)}</strong>
            </section>
          </section>
        </Panel>
      )}

      <DataTable
        title="Itens calculados"
        columns={['Colaborador', 'Rubrica', 'Tipo', 'Origem', 'Valor', 'Quantidade', 'Base', 'Aliquota']}
        rows={calculation.items.map((item) => [
          `${item.employeeRegistration} - ${item.employeeName}`,
          `${item.rubricCode} - ${item.rubricName}`,
          labelRubricType(item.rubricType),
          labelCalculationOrigin(item.origin),
          formatCurrency(item.amount),
          item.quantity ? formatNumber(item.quantity) : '-',
          item.baseAmount ? formatCurrency(item.baseAmount) : '-',
          item.calculationRate ? `${formatNumber(item.calculationRate)}%` : '-',
        ])}
      />
    </>
  )
}

function PayslipList({ title, items }: { title: string; items: PayrollCalculationItem[] }) {
  return (
    <div className="payslip-list">
      <h4>{title}</h4>
      {items.length === 0 ? (
        <span className="payslip-empty">Sem itens</span>
      ) : (
        <ul>
          {items.map((item) => (
            <li key={item.id}>
              <div>
                <strong>{item.rubricName}</strong>
                <span>{item.rubricCode} - {labelCalculationOrigin(item.origin)}</span>
              </div>
              <span>{formatCurrency(item.amount)}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

function ReportsView({
  employees,
  payrollCalculations,
}: {
  employees: Employee[]
  payrollCalculations: PayrollCalculation[]
}) {
  const [selectedCalculationId, setSelectedCalculationId] = useState(payrollCalculations[0]?.id ?? '')
  const [calculationDetail, setCalculationDetail] = useState<PayrollCalculationDetail | null>(null)
  const [reportState, setReportState] = useState<LoadState>('ready')

  const selectedCalculation =
    payrollCalculations.find((calculation) => calculation.id === selectedCalculationId) ?? payrollCalculations[0]

  useEffect(() => {
    if (selectedCalculationId || payrollCalculations.length === 0) return

    setSelectedCalculationId(payrollCalculations[0].id)
  }, [payrollCalculations, selectedCalculationId])

  useEffect(() => {
    if (!selectedCalculation) {
      setCalculationDetail(null)
      return
    }

    let isCurrent = true

    async function loadCalculationDetail() {
      setReportState('loading')
      try {
        const response = await apiFetch(`/payroll-calculations/period/${selectedCalculation.payrollPeriodId}/current`)

        if (!response.ok) {
          throw new Error('Calculo nao encontrado')
        }

        const detail = await response.json()
        if (isCurrent) {
          setCalculationDetail(detail)
          setReportState('ready')
        }
      } catch {
        if (isCurrent) {
          setCalculationDetail(null)
          setReportState('error')
        }
      }
    }

    void loadCalculationDetail()

    return () => {
      isCurrent = false
    }
  }, [selectedCalculation])

  const employeeSummaries = useMemo(
    () => summarizePayrollByEmployee(calculationDetail?.items ?? []),
    [calculationDetail],
  )

  const rubricRows = useMemo(
    () => summarizePayrollByRubric(calculationDetail?.items ?? []),
    [calculationDetail],
  )

  const sectorRows = useMemo(
    () => summarizePayrollByStructure(calculationDetail?.items ?? [], employees),
    [calculationDetail, employees],
  )

  const totalFgts = employeeSummaries.reduce((total, summary) => total + summary.fgts, 0)
  const totalInss = employeeSummaries.reduce((total, summary) => total + summary.inss, 0)
  const totalIrrf = employeeSummaries.reduce((total, summary) => total + summary.irrf, 0)

  function exportEmployeeReport() {
    exportCsv(`relatorio-colaboradores-${calculationDetail?.periodCode ?? 'folha'}.csv`, [
      ['Competencia', 'Matricula', 'Colaborador', 'Proventos', 'Descontos', 'Liquido', 'Base FGTS', 'FGTS', 'Base INSS', 'INSS', 'Base IRRF', 'IRRF'],
      ...employeeSummaries.map((summary) => [
        calculationDetail?.periodCode ?? '',
        summary.registration,
        summary.name,
        summary.proventos,
        summary.descontos,
        summary.liquido,
        summary.baseFgts,
        summary.fgts,
        summary.baseInss,
        summary.inss,
        summary.baseIrrf,
        summary.irrf,
      ]),
    ])
  }

  function exportRubricReport() {
    exportCsv(`relatorio-rubricas-${calculationDetail?.periodCode ?? 'folha'}.csv`, [
      ['Competencia', 'Codigo', 'Rubrica', 'Tipo', 'Itens', 'Valor'],
      ...rubricRows.map((row) => [
        calculationDetail?.periodCode ?? '',
        row.code,
        row.name,
        labelRubricType(row.type),
        row.items,
        row.amount,
      ]),
    ])
  }

  function exportStructureReport() {
    exportCsv(`relatorio-setores-cargos-${calculationDetail?.periodCode ?? 'folha'}.csv`, [
      ['Competencia', 'Setor', 'Cargo', 'Colaboradores', 'Proventos', 'Descontos', 'Liquido'],
      ...sectorRows.map((row) => [
        calculationDetail?.periodCode ?? '',
        row.department,
        row.jobPosition,
        row.employees,
        row.proventos,
        row.descontos,
        row.liquido,
      ]),
    ])
  }

  if (payrollCalculations.length === 0) {
    return (
      <Panel title="Relatorios da folha">
        <div className="empty-state">
          <span>Nenhuma folha calculada para relatorio.</span>
        </div>
      </Panel>
    )
  }

  return (
    <>
      <Panel title="Filtros dos relatorios">
        <div className="report-toolbar">
          <label>
            Folha
            <select value={selectedCalculation?.id ?? ''} onChange={(event) => setSelectedCalculationId(event.target.value)}>
              {payrollCalculations.map((calculation) => (
                <option key={calculation.id} value={calculation.id}>
                  {calculation.periodCode} - {formatDateTime(calculation.calculatedAt)}
                </option>
              ))}
            </select>
          </label>
          <div>
            <span className={`status-badge status-${calculationDetail?.status ?? 'em_calculo'}`}>
              {calculationDetail ? labelPeriodStatus(calculationDetail.status) : 'Carregando'}
            </span>
          </div>
        </div>
      </Panel>

      {reportState === 'error' && <div className="alert">Nao foi possivel carregar os dados do relatorio.</div>}

      <section className="metrics-grid">
        <Metric label="Colaboradores" value={calculationDetail?.employeeCount ?? 0} />
        <Metric label="Proventos" value={formatCurrency(calculationDetail?.totalProventos ?? 0)} />
        <Metric label="Descontos" value={formatCurrency(calculationDetail?.totalDescontos ?? 0)} />
        <Metric label="Liquido" value={formatCurrency(calculationDetail?.totalLiquido ?? 0)} />
        <Metric label="FGTS" value={formatCurrency(totalFgts)} />
        <Metric label="INSS" value={formatCurrency(totalInss)} />
        <Metric label="IRRF" value={formatCurrency(totalIrrf)} />
      </section>

      <ReportSection title="Relatorio por colaborador" onExport={exportEmployeeReport} disabled={!calculationDetail}>
        <DataTable
          title="Totais por colaborador"
          columns={['Colaborador', 'Proventos', 'Descontos', 'Liquido', 'FGTS', 'INSS', 'IRRF']}
          rows={employeeSummaries.map((summary) => [
            summary.employee,
            formatCurrency(summary.proventos),
            formatCurrency(summary.descontos),
            formatCurrency(summary.liquido),
            formatCurrency(summary.fgts),
            formatCurrency(summary.inss),
            formatCurrency(summary.irrf),
          ])}
        />
      </ReportSection>

      <ReportSection title="Relatorio por rubrica" onExport={exportRubricReport} disabled={!calculationDetail}>
        <DataTable
          title="Totais por rubrica"
          columns={['Codigo', 'Rubrica', 'Tipo', 'Itens', 'Valor']}
          rows={rubricRows.map((row) => [
            row.code,
            row.name,
            labelRubricType(row.type),
            String(row.items),
            formatCurrency(row.amount),
          ])}
        />
      </ReportSection>

      <ReportSection title="Relatorio por setor e cargo" onExport={exportStructureReport} disabled={!calculationDetail}>
        <DataTable
          title="Totais por estrutura"
          columns={['Setor', 'Cargo', 'Colaboradores', 'Proventos', 'Descontos', 'Liquido']}
          rows={sectorRows.map((row) => [
            row.department,
            row.jobPosition,
            String(row.employees),
            formatCurrency(row.proventos),
            formatCurrency(row.descontos),
            formatCurrency(row.liquido),
          ])}
        />
      </ReportSection>
    </>
  )
}

function ReportSection({
  title,
  onExport,
  disabled,
  children,
}: {
  title: string
  onExport: () => void
  disabled: boolean
  children: ReactNode
}) {
  return (
    <section className="report-section">
      <div className="report-section-header">
        <h3>{title}</h3>
        <button type="button" onClick={onExport} disabled={disabled}>
          Exportar CSV
        </button>
      </div>
      {children}
    </section>
  )
}

function openPayslipPrintWindow(
  periodCode: string,
  summary: {
    registration: string
    name: string
    proventos: number
    descontos: number
    liquido: number
    baseFgts: number
    fgts: number
    baseInss: number
    inss: number
    baseIrrf: number
    irrf: number
  },
  proventos: PayrollCalculationItem[],
  descontos: PayrollCalculationItem[],
  informativas: PayrollCalculationItem[],
) {
  const printWindow = window.open('', '_blank', 'width=900,height=700')

  if (!printWindow) {
    return
  }

  printWindow.document.write(buildPayslipHtml(periodCode, summary, proventos, descontos, informativas))
  printWindow.document.close()
  printWindow.focus()
  printWindow.setTimeout(() => {
    printWindow.print()
  }, 250)
}

function openAllPayslipsPrintWindow(
  periodCode: string,
  summaries: Array<{
    employeeId: string
    registration: string
    name: string
    proventos: number
    descontos: number
    liquido: number
    baseFgts: number
    fgts: number
    baseInss: number
    inss: number
    baseIrrf: number
    irrf: number
  }>,
  items: PayrollCalculationItem[],
) {
  const printWindow = window.open('', '_blank', 'width=900,height=700')

  if (!printWindow) {
    return
  }

  printWindow.document.write(buildAllPayslipsHtml(periodCode, summaries, items))
  printWindow.document.close()
  printWindow.focus()
  printWindow.setTimeout(() => {
    printWindow.print()
  }, 250)
}

function buildPayslipHtml(
  periodCode: string,
  summary: {
    registration: string
    name: string
    proventos: number
    descontos: number
    liquido: number
    baseFgts: number
    fgts: number
    baseInss: number
    inss: number
    baseIrrf: number
    irrf: number
  },
  proventos: PayrollCalculationItem[],
  descontos: PayrollCalculationItem[],
  informativas: PayrollCalculationItem[],
) {
  return `
    <!doctype html>
    <html lang="pt-BR">
      <head>
        <meta charset="utf-8" />
        <title>Holerite ${escapeHtml(periodCode)} - ${escapeHtml(summary.name)}</title>
        ${payslipPrintStyles()}
      </head>
      <body>
        ${buildPayslipBody(periodCode, summary, proventos, descontos, informativas)}
      </body>
    </html>
  `
}

function buildAllPayslipsHtml(
  periodCode: string,
  summaries: Array<{
    employeeId: string
    registration: string
    name: string
    proventos: number
    descontos: number
    liquido: number
    baseFgts: number
    fgts: number
    baseInss: number
    inss: number
    baseIrrf: number
    irrf: number
  }>,
  items: PayrollCalculationItem[],
) {
  const sections = summaries
    .map((summary) => {
      const employeeItems = items.filter((item) => item.employeeId === summary.employeeId)
      const proventos = employeeItems.filter((item) => item.rubricType === 'provento')
      const descontos = employeeItems.filter((item) => item.rubricType === 'desconto')
      const informativas = employeeItems.filter((item) => item.rubricType === 'informativa')

      return buildPayslipBody(periodCode, summary, proventos, descontos, informativas)
    })
    .join('')

  return `
    <!doctype html>
    <html lang="pt-BR">
      <head>
        <meta charset="utf-8" />
        <title>Holerites ${escapeHtml(periodCode)}</title>
        ${payslipPrintStyles()}
      </head>
      <body>
        ${sections}
      </body>
    </html>
  `
}

function buildPayslipBody(
  periodCode: string,
  summary: {
    registration: string
    name: string
    proventos: number
    descontos: number
    liquido: number
    baseFgts: number
    fgts: number
    baseInss: number
    inss: number
    baseIrrf: number
    irrf: number
  },
  proventos: PayrollCalculationItem[],
  descontos: PayrollCalculationItem[],
  informativas: PayrollCalculationItem[],
) {
  const itemRows = (items: PayrollCalculationItem[]) =>
    items.length === 0
      ? '<tr><td colspan="3">Sem itens</td></tr>'
      : items
          .map(
            (item) => `
              <tr>
                <td>${escapeHtml(item.rubricCode)}</td>
                <td>${escapeHtml(item.rubricName)}</td>
                <td class="money">${formatCurrency(item.amount)}</td>
              </tr>
            `,
          )
          .join('')

  return `
    <main class="payslip-page">
      <header>
        <div>
          <span class="label">Holerite</span>
          <h1>Competencia ${escapeHtml(periodCode)}</h1>
          <h2>RH Folha</h2>
        </div>
        <div class="employee">
          <span>Colaborador</span>
          <h1>${escapeHtml(summary.name)}</h1>
          <h2>Matricula ${escapeHtml(summary.registration)}</h2>
        </div>
      </header>

      <section class="bases">
        <div class="base"><span class="label">Base FGTS</span><strong>${formatCurrency(summary.baseFgts)}</strong></div>
        <div class="base"><span class="label">FGTS</span><strong>${formatCurrency(summary.fgts)}</strong></div>
        <div class="base"><span class="label">Base INSS</span><strong>${formatCurrency(summary.baseInss)}</strong></div>
        <div class="base"><span class="label">INSS</span><strong>${formatCurrency(summary.inss)}</strong></div>
        <div class="base"><span class="label">Base IRRF</span><strong>${formatCurrency(summary.baseIrrf)}</strong></div>
        <div class="base"><span class="label">IRRF</span><strong>${formatCurrency(summary.irrf)}</strong></div>
      </section>

      <section class="columns">
        <div>
          <h3>Proventos</h3>
          <table><thead><tr><th>Cod.</th><th>Rubrica</th><th>Valor</th></tr></thead><tbody>${itemRows(proventos)}</tbody></table>
        </div>
        <div>
          <h3>Descontos</h3>
          <table><thead><tr><th>Cod.</th><th>Rubrica</th><th>Valor</th></tr></thead><tbody>${itemRows(descontos)}</tbody></table>
        </div>
        <div>
          <h3>Informativas</h3>
          <table><thead><tr><th>Cod.</th><th>Rubrica</th><th>Valor</th></tr></thead><tbody>${itemRows(informativas)}</tbody></table>
        </div>
      </section>

      <section class="totals">
        <span>Proventos: ${formatCurrency(summary.proventos)}</span>
        <span>Descontos: ${formatCurrency(summary.descontos)}</span>
        <strong>Liquido: ${formatCurrency(summary.liquido)}</strong>
      </section>
    </main>
  `
}

function payslipPrintStyles() {
  return `
    <style>
      * { box-sizing: border-box; }
      body { color: #111827; font-family: Arial, sans-serif; margin: 24px; }
      .payslip-page { break-after: page; page-break-after: always; }
      .payslip-page:last-child { break-after: auto; page-break-after: auto; }
      header { border-bottom: 2px solid #111827; display: flex; justify-content: space-between; gap: 16px; padding-bottom: 14px; }
      h1, h2, h3 { margin: 0; }
      h1 { font-size: 22px; }
      h2 { font-size: 16px; margin-top: 4px; }
      section { margin-top: 18px; }
      .employee { text-align: right; }
      .employee span, .label { color: #4b5563; display: block; font-size: 12px; font-weight: 700; text-transform: uppercase; }
      .bases { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; }
      .base { border: 1px solid #9ca3af; padding: 10px; }
      .base strong { display: block; font-size: 16px; margin-top: 4px; }
      .columns { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; }
      table { border-collapse: collapse; width: 100%; }
      th, td { border-bottom: 1px solid #d1d5db; font-size: 12px; padding: 8px 6px; text-align: left; }
      th { background: #f3f4f6; text-transform: uppercase; }
      .money { text-align: right; white-space: nowrap; }
      .totals { align-items: center; border-top: 2px solid #111827; display: flex; gap: 18px; justify-content: flex-end; padding-top: 14px; }
      .totals strong { font-size: 18px; }
      @media print {
        body { margin: 12mm; }
        .payslip-page { min-height: 260mm; }
      }
    </style>
  `
}

function escapeHtml(value: string) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;')
}

function RubricsView({
  canEdit,
  activeCompany,
  rubrics,
  onSubmit,
  onToggleStatus,
}: {
  canEdit: boolean
  activeCompany: Company | undefined
  rubrics: Rubric[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onToggleStatus: (rubric: Rubric) => void
}) {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    await Promise.resolve(onSubmit(event))
    setIsCreateModalOpen(false)
  }

  return (
    <>
      {canEdit && (
        <section className="section-actions">
          <button type="button" onClick={() => setIsCreateModalOpen(true)}>
            Nova rubrica
          </button>
        </section>
      )}

      {isCreateModalOpen && (
        <Modal title="Nova rubrica" onClose={() => setIsCreateModalOpen(false)} size="wide">
        <form className="rubric-form" onSubmit={(event) => void handleSubmit(event)}>
          <label>
            Codigo
            <input name="code" placeholder="Ex: 204" required />
          </label>
          <label>
            Nome
            <input name="name" placeholder="Ex: Gratificacao" required />
          </label>
          <label>
            Tipo
            <select name="type" required>
              <option value="provento">Provento</option>
              <option value="desconto">Desconto</option>
              <option value="informativa">Informativa</option>
              <option value="base">Base</option>
              <option value="patronal">Patronal</option>
            </select>
          </label>
          <label>
            Natureza eSocial
            <input name="eSocialNature" placeholder="Opcional" />
          </label>
          <label>
            Inicio vigencia
            <input name="startsOn" type="date" defaultValue="2026-01-01" required />
          </label>
          <label>
            Forma de calculo
            <select name="calculationMethod" required>
              <option value="valor_fixo">Valor fixo</option>
              <option value="percentual">Percentual</option>
              <option value="quantidade_valor">Quantidade x valor</option>
              <option value="manual">Manual</option>
              <option value="sistema">Sistema</option>
            </select>
          </label>
          <label>
            Base de calculo
            <select name="calculationBase" required>
              <option value="nenhuma">Nenhuma</option>
              <option value="salario_base">Salario base</option>
              <option value="salario_bruto">Salario bruto</option>
              <option value="base_inss">Base INSS</option>
              <option value="base_irrf">Base IRRF</option>
              <option value="base_fgts">Base FGTS</option>
              <option value="producao">Producao</option>
            </select>
          </label>
          <label className="span-2">
            Descricao
            <input name="description" placeholder="Observacao operacional opcional" />
          </label>
          <div className="check-group">
            <label>
              <input name="allowsManualEntry" type="checkbox" defaultChecked />
              Manual
            </label>
            <label>
              <input name="allowsMassEntry" type="checkbox" />
              Massa
            </label>
            <label>
              <input name="allowsFixedEntry" type="checkbox" />
              Fixo
            </label>
            <label>
              <input name="incidenceInss" type="checkbox" />
              INSS
            </label>
            <label>
              <input name="incidenceFgts" type="checkbox" />
              FGTS
            </label>
            <label>
              <input name="incidenceIrrf" type="checkbox" />
              IRRF
            </label>
            <label>
              <input name="incidenceDsr" type="checkbox" />
              DSR
            </label>
          </div>
          <div className="modal-actions">
            <button type="button" onClick={() => setIsCreateModalOpen(false)}>
              Cancelar
            </button>
            <button type="submit" disabled={!activeCompany}>
              Salvar rubrica
            </button>
          </div>
        </form>
        </Modal>
      )}

      <DataTable
        title="Rubricas cadastradas"
        columns={canEdit ? ['Codigo', 'Nome', 'Tipo', 'Calculo', 'Incidencias', 'Uso', 'Status', 'Acoes'] : ['Codigo', 'Nome', 'Tipo', 'Calculo', 'Incidencias', 'Uso', 'Status']}
        rows={rubrics.map((rubric) => [
          rubric.code,
          rubric.name,
          labelRubricType(rubric.type),
          rubric.currentValidity?.calculationMethod ?? '-',
          formatIncidences(rubric),
          formatRubricUsage(rubric),
          activeBadge(rubric.isActive),
          ...(canEdit ? [actionButton(rubric.isActive ? 'Inativar' : 'Reativar', () => onToggleStatus(rubric))] : []),
        ])}
      />
    </>
  )
}

function EmployeesView({
  userRole,
  activeCompany,
  departments,
  jobPositions,
  employees,
  rubrics,
  fixedPayrollEntries,
  onSubmit,
  onFixedSubmit,
  onFixedUpdate,
  onFixedClose,
  onFixedToggle,
  onRefresh,
}: {
  userRole: string
  activeCompany: Company | undefined
  departments: Department[]
  jobPositions: JobPosition[]
  employees: Employee[]
  rubrics: Rubric[]
  fixedPayrollEntries: FixedPayrollEntry[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onFixedSubmit: (event: FormEvent<HTMLFormElement>) => void
  onFixedUpdate: (entry: FixedPayrollEntry, event: FormEvent<HTMLFormElement>) => void
  onFixedClose: (entry: FixedPayrollEntry, event: FormEvent<HTMLFormElement>) => void
  onFixedToggle: (entry: FixedPayrollEntry) => void
  onRefresh: () => Promise<void>
}) {
  const [selectedEmployeeId, setSelectedEmployeeId] = useState('')
  const [employeeEvents, setEmployeeEvents] = useState<EmployeeEvent[]>([])
  const [eventState, setEventState] = useState<LoadState>('ready')
  const [employeeMessage, setEmployeeMessage] = useState('')
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isProfileModalOpen, setIsProfileModalOpen] = useState(false)
  const [isFixedFormOpen, setIsFixedFormOpen] = useState(false)
  const [editingFixedEntry, setEditingFixedEntry] = useState<FixedPayrollEntry | null>(null)
  const [closingFixedEntry, setClosingFixedEntry] = useState<FixedPayrollEntry | null>(null)
  const canEdit = canOperateHr(userRole)
  const [employeeFilters, setEmployeeFilters] = useState({
    search: '',
    departmentId: 'todos',
    jobPositionId: 'todos',
    responsibleEmployeeId: 'todos',
    compensationModel: 'todos',
    admissionFrom: '',
    admissionTo: '',
  })
  const selectedEmployee = employees.find((employee) => employee.id === selectedEmployeeId)
  const fixedRubrics = rubrics.filter((rubric) => rubric.allowsManualEntry && rubric.allowsFixedEntry && rubric.isActive)
  const selectedEmployeeFixedEntries = fixedPayrollEntries.filter((entry) => entry.employeeId === selectedEmployee?.id)
  const defaultFixedRubricId = selectDefaultValue(fixedRubrics)
  const filteredEmployees = useMemo(
    () =>
      employees.filter((employee) => {
        const search = normalizeSearch(employeeFilters.search)
        const matchesSearch =
          search.length === 0 ||
          normalizeSearch(employee.name).includes(search) ||
          normalizeSearch(employee.registration).includes(search) ||
          normalizeSearch(employee.documentNumber).includes(search)
        const matchesDepartment =
          employeeFilters.departmentId === 'todos' || employee.departmentId === employeeFilters.departmentId
        const matchesJobPosition =
          employeeFilters.jobPositionId === 'todos' || employee.jobPositionId === employeeFilters.jobPositionId
        const matchesResponsible =
          employeeFilters.responsibleEmployeeId === 'todos' ||
          employee.responsibleEmployeeId === employeeFilters.responsibleEmployeeId
        const matchesModel =
          employeeFilters.compensationModel === 'todos' || employee.compensationModel === employeeFilters.compensationModel
        const matchesAdmissionFrom =
          !employeeFilters.admissionFrom || employee.admissionDate >= employeeFilters.admissionFrom
        const matchesAdmissionTo = !employeeFilters.admissionTo || employee.admissionDate <= employeeFilters.admissionTo

        return (
          matchesSearch &&
          matchesDepartment &&
          matchesJobPosition &&
          matchesResponsible &&
          matchesModel &&
          matchesAdmissionFrom &&
          matchesAdmissionTo
        )
      }),
    [employeeFilters, employees],
  )

  useEffect(() => {
    if (!selectedEmployee || !isProfileModalOpen) {
      setEmployeeEvents([])
      return
    }

    let isCurrent = true
    const employeeId = selectedEmployee.id

    async function loadEmployeeEvents() {
      setEventState('loading')
      try {
        const response = await apiFetch(`/employees/${employeeId}/events`)

        if (!response.ok) {
          throw new Error('Nao foi possivel carregar eventos')
        }

        const events = await response.json()
        if (isCurrent) {
          setEmployeeEvents(events)
          setEventState('ready')
        }
      } catch {
        if (isCurrent) {
          setEmployeeEvents([])
          setEventState('error')
        }
      }
    }

    void loadEmployeeEvents()

    return () => {
      isCurrent = false
    }
  }, [isProfileModalOpen, selectedEmployee])

  async function handleCreateEmployeeSubmit(event: FormEvent<HTMLFormElement>) {
    await Promise.resolve(onSubmit(event))
    setIsCreateModalOpen(false)
  }

  async function handlePhotoSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedEmployee) return

    const form = event.currentTarget
    const data = new FormData(form)
    const file = data.get('file')

    if (!(file instanceof File) || file.size === 0) {
      setEmployeeMessage('Selecione uma foto para enviar.')
      return
    }

    try {
      const uploadData = new FormData()
      uploadData.append('file', file)

      const response = await apiFetch(`/employees/${selectedEmployee.id}/photo-upload`, {
        method: 'POST',
        body: uploadData,
      })

      if (!response.ok) {
        throw new Error('Nao foi possivel atualizar foto')
      }

      await onRefresh()
      form.reset()
      setEmployeeMessage('Foto atualizada.')
    } catch {
      setEmployeeMessage('Nao foi possivel atualizar a foto.')
    }
  }

  async function handleResponsibleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedEmployee) return

    const form = event.currentTarget
    const data = new FormData(form)

    try {
      const response = await apiFetch(`/employees/${selectedEmployee.id}/responsible`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ responsibleEmployeeId: data.get('responsibleEmployeeId') || null }),
      })

      if (!response.ok) {
        throw new Error('Nao foi possivel atualizar responsavel')
      }

      await onRefresh()
      setEmployeeMessage('Responsavel atualizado.')
    } catch {
      setEmployeeMessage('Nao foi possivel atualizar o responsavel.')
    }
  }

  async function handleEmployeeFixedSubmit(event: FormEvent<HTMLFormElement>) {
    await Promise.resolve(onFixedSubmit(event))
    setIsFixedFormOpen(false)
  }

  async function handleEmployeeEventSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedEmployee) return

    const form = event.currentTarget
    const data = new FormData(form)

    try {
      const response = await apiFetch(`/employees/${selectedEmployee.id}/events`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          eventDate: data.get('eventDate'),
          type: data.get('type'),
          title: data.get('title'),
          description: data.get('description'),
          responsible: data.get('responsible'),
        }),
      })

      if (!response.ok) {
        throw new Error('Nao foi possivel salvar evento')
      }

      form.reset()
      setEmployeeMessage('Evento registrado no acompanhamento.')
      setSelectedEmployeeId(selectedEmployee.id)
      const eventsResponse = await apiFetch(`/employees/${selectedEmployee.id}/events`)
      setEmployeeEvents(await eventsResponse.json())
    } catch {
      setEmployeeMessage('Nao foi possivel registrar o evento.')
    }
  }

  async function handleCancelEmployeeEvent(eventId: string) {
    try {
      const response = await apiFetch(`/employees/events/${eventId}/cancel`, {
        method: 'POST',
      })

      if (!response.ok) {
        throw new Error('Nao foi possivel cancelar evento')
      }

      setEmployeeEvents((current) =>
        current.map((item) => (item.id === eventId ? { ...item, status: 'cancelado' } : item)),
      )
      setEmployeeMessage('Evento cancelado e preservado no historico.')
    } catch {
      setEmployeeMessage('Nao foi possivel cancelar o evento.')
    }
  }

  return (
    <>
      <Panel title="Pesquisar colaboradores">
        <div className="employee-toolbar">
          <span>{filteredEmployees.length} colaborador(es) encontrado(s)</span>
          {canEdit && (
            <button type="button" onClick={() => setIsCreateModalOpen(true)}>
              Novo colaborador
            </button>
          )}
        </div>
        <form className="employee-filter-form">
          <label>
            Nome, matricula ou CPF
            <input
              value={employeeFilters.search}
              onChange={(event) => setEmployeeFilters((current) => ({ ...current, search: event.target.value }))}
              placeholder="Digite para filtrar"
            />
          </label>
          <label>
            Setor
            <select
              value={employeeFilters.departmentId}
              onChange={(event) => setEmployeeFilters((current) => ({ ...current, departmentId: event.target.value }))}
            >
              <option value="todos">Todos</option>
              {departments.map((department) => (
                <option key={department.id} value={department.id}>
                  {department.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Cargo
            <select
              value={employeeFilters.jobPositionId}
              onChange={(event) => setEmployeeFilters((current) => ({ ...current, jobPositionId: event.target.value }))}
            >
              <option value="todos">Todos</option>
              {jobPositions.map((jobPosition) => (
                <option key={jobPosition.id} value={jobPosition.id}>
                  {jobPosition.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Responsavel
            <select
              value={employeeFilters.responsibleEmployeeId}
              onChange={(event) =>
                setEmployeeFilters((current) => ({ ...current, responsibleEmployeeId: event.target.value }))
              }
            >
              <option value="todos">Todos</option>
              {employees.map((employee) => (
                <option key={employee.id} value={employee.id}>
                  {employee.registration} - {employee.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Modelo
            <select
              value={employeeFilters.compensationModel}
              onChange={(event) =>
                setEmployeeFilters((current) => ({ ...current, compensationModel: event.target.value }))
              }
            >
              <option value="todos">Todos</option>
              <option value="mensalista">Mensalista</option>
              <option value="producao">Producao</option>
            </select>
          </label>
          <label>
            Admitido de
            <input
              type="date"
              value={employeeFilters.admissionFrom}
              onChange={(event) => setEmployeeFilters((current) => ({ ...current, admissionFrom: event.target.value }))}
            />
          </label>
          <label>
            Admitido ate
            <input
              type="date"
              value={employeeFilters.admissionTo}
              onChange={(event) => setEmployeeFilters((current) => ({ ...current, admissionTo: event.target.value }))}
            />
          </label>
          <button
            type="button"
            onClick={() =>
              setEmployeeFilters({
                search: '',
                departmentId: 'todos',
                jobPositionId: 'todos',
                responsibleEmployeeId: 'todos',
                compensationModel: 'todos',
                admissionFrom: '',
                admissionTo: '',
              })
            }
          >
            Limpar filtros
          </button>
        </form>
      </Panel>

      <DataTable
        title={`Colaboradores (${filteredEmployees.length})`}
        columns={['Matricula', 'Nome', 'Setor', 'Cargo', 'Responsavel', 'Modelo', 'Base', 'Acoes']}
        rows={filteredEmployees.map((employee) => [
          employee.registration,
          employee.name,
          employee.departmentName,
          employee.jobPositionName,
          employee.responsibleEmployeeName ?? '-',
          labelCompensation(employee.compensationModel),
          employee.compensationModel === 'producao'
            ? formatCurrency(employee.productionUnitValue)
            : formatCurrency(employee.baseSalary),
          actionButton('Ficha', () => {
            setSelectedEmployeeId(employee.id)
            setIsProfileModalOpen(true)
          }),
        ])}
      />

      {employeeMessage && <FeedbackBanner message={{ text: employeeMessage, type: inferFeedbackType(employeeMessage) }} />}
      {eventState === 'error' && <div className="alert">Nao foi possivel carregar o acompanhamento do colaborador.</div>}

      {isCreateModalOpen && (
        <Modal title="Novo colaborador" onClose={() => setIsCreateModalOpen(false)}>
          <form className="employee-form" onSubmit={(event) => void handleCreateEmployeeSubmit(event)}>
            <label>
              Matricula
              <input name="registration" placeholder="0004" required />
            </label>
            <label>
              Nome
              <input name="name" placeholder="Nome completo" required />
            </label>
            <label>
              CPF
              <input name="documentNumber" placeholder="Somente numeros" required />
            </label>
            <label>
              Admissao
              <input name="admissionDate" type="date" required />
            </label>
            <label>
              Setor
              <select name="departmentId" required>
                {departments.map((department) => (
                  <option key={department.id} value={department.id}>
                    {department.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Cargo
              <select name="jobPositionId" required>
                {jobPositions.map((jobPosition) => (
                  <option key={jobPosition.id} value={jobPosition.id}>
                    {jobPosition.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Modelo
              <select name="compensationModel" required>
                <option value="mensalista">Mensalista</option>
                <option value="producao">Producao</option>
              </select>
            </label>
            <label>
              Salario base
              <input name="baseSalary" type="number" min="0" step="0.01" defaultValue="0" />
            </label>
            <label>
              Valor producao
              <input name="productionUnitValue" type="number" min="0" step="0.01" defaultValue="0" />
            </label>
            <label>
              Responsavel direto
              <select name="responsibleEmployeeId">
                <option value="">Sem responsavel</option>
                {employees.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {employee.registration} - {employee.name}
                  </option>
                ))}
              </select>
            </label>
            <div className="modal-actions">
              <button type="button" onClick={() => setIsCreateModalOpen(false)}>
                Cancelar
              </button>
              <button type="submit" disabled={!activeCompany || departments.length === 0 || jobPositions.length === 0}>
                Salvar colaborador
              </button>
            </div>
          </form>
        </Modal>
      )}

      {selectedEmployee && isProfileModalOpen && (
        <Modal title="Ficha e acompanhamento" onClose={() => setIsProfileModalOpen(false)} size="wide">
          <section className="employee-profile">
            <div className="employee-photo">
              {selectedEmployee.photoUrl ? (
                <img src={assetUrl(selectedEmployee.photoUrl)} alt={`Foto de ${selectedEmployee.name}`} />
              ) : (
                <span>{initials(selectedEmployee.name)}</span>
              )}
            </div>

            <div className="employee-profile-info">
              <p className="eyebrow">Colaborador selecionado</p>
              <h3>{selectedEmployee.name}</h3>
              <dl>
                <div>
                  <dt>Matricula</dt>
                  <dd>{selectedEmployee.registration}</dd>
                </div>
                <div>
                  <dt>Setor</dt>
                  <dd>{selectedEmployee.departmentName}</dd>
                </div>
                <div>
                  <dt>Cargo</dt>
                  <dd>{selectedEmployee.jobPositionName}</dd>
                </div>
                <div>
                  <dt>Responsavel</dt>
                  <dd>{selectedEmployee.responsibleEmployeeName ?? 'Sem responsavel'}</dd>
                </div>
                <div>
                  <dt>Admissao</dt>
                  <dd>{formatDate(selectedEmployee.admissionDate)}</dd>
                </div>
              </dl>
            </div>

            {canEdit && (
              <form className="employee-photo-form" onSubmit={(event) => void handlePhotoSubmit(event)}>
                <label>
                  Foto
                  <input name="file" type="file" accept="image/png,image/jpeg,image/webp" required />
                </label>
                <button type="submit">Enviar foto</button>
              </form>
            )}
          </section>

          {canEdit && (
            <section className="employee-responsible-panel">
            <form className="employee-responsible-form" onSubmit={(event) => void handleResponsibleSubmit(event)}>
              <label>
                Responsavel direto
                <select name="responsibleEmployeeId" defaultValue={selectedEmployee.responsibleEmployeeId ?? ''}>
                  <option value="">Sem responsavel</option>
                  {employees
                    .filter((employee) => employee.id !== selectedEmployee.id)
                    .map((employee) => (
                      <option key={employee.id} value={employee.id}>
                        {employee.registration} - {employee.name}
                      </option>
                    ))}
                </select>
              </label>
              <button type="submit">Atualizar responsavel</button>
            </form>
            </section>
          )}

          <section className="employee-fixed-panel">
            <div className="employee-section-header">
              <div>
                <h4>Lancamentos fixos</h4>
                <span>{selectedEmployeeFixedEntries.length} registro(s) vinculado(s)</span>
              </div>
              {canEdit && (
                <button type="button" onClick={() => setIsFixedFormOpen((current) => !current)}>
                  {isFixedFormOpen ? 'Ocultar novo fixo' : 'Novo fixo para este colaborador'}
                </button>
              )}
            </div>

            {canEdit && isFixedFormOpen && (
              <form className="entry-form" onSubmit={(event) => void handleEmployeeFixedSubmit(event)}>
                <input name="employeeId" type="hidden" value={selectedEmployee.id} />
                <label>
                  Colaborador
                  <input value={`${selectedEmployee.registration} - ${selectedEmployee.name}`} disabled />
                </label>
                <label>
                  Rubrica
                  <select name="rubricId" defaultValue={defaultFixedRubricId} required>
                    {fixedRubrics.length !== 1 && (
                      <option value="" disabled>
                        Selecione a rubrica fixa
                      </option>
                    )}
                    {fixedRubrics.map((rubric) => (
                      <option key={rubric.id} value={rubric.id}>
                        {rubric.code} - {rubric.name}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  Inicio vigencia
                  <input name="startsOn" type="date" required />
                </label>
                <label>
                  Fim vigencia
                  <input name="endsOn" type="date" />
                </label>
                <label>
                  Valor
                  <input name="amount" type="number" min="0.01" step="0.01" required />
                </label>
                <label>
                  Quantidade
                  <input name="quantity" type="number" min="0" step="0.0001" placeholder="Opcional" />
                </label>
                <label className="span-2">
                  Observacao
                  <input name="notes" placeholder="Ex: desconto mensal recorrente" />
                </label>
                <div className="modal-actions">
                  <button type="button" onClick={() => setIsFixedFormOpen(false)}>
                    Cancelar
                  </button>
                  <button type="submit" disabled={!activeCompany || fixedRubrics.length === 0}>
                    Salvar fixo
                  </button>
                </div>
              </form>
            )}

            <DataTable
              title="Fixos vinculados"
              columns={canEdit ? ['Rubrica', 'Tipo', 'Valor', 'Quantidade', 'Vigencia', 'Status', 'Observacao', 'Acoes'] : ['Rubrica', 'Tipo', 'Valor', 'Quantidade', 'Vigencia', 'Status', 'Observacao']}
              rows={selectedEmployeeFixedEntries.map((entry) => [
                `${entry.rubricCode} - ${entry.rubricName}`,
                labelRubricType(entry.rubricType),
                formatCurrency(entry.amount),
                entry.quantity ? formatNumber(entry.quantity) : '-',
                `${formatDate(entry.startsOn)} a ${entry.endsOn ? formatDate(entry.endsOn) : 'sem fim'}`,
                activeBadge(entry.isActive),
                entry.notes ?? '-',
                ...(canEdit ? [fixedPayrollEntryActions(entry, setEditingFixedEntry, setClosingFixedEntry, onFixedToggle)] : []),
              ])}
            />
          </section>

          {canEdit && (
            <form className="employee-event-form" onSubmit={(event) => void handleEmployeeEventSubmit(event)}>
            <label>
              Data
              <input name="eventDate" type="date" defaultValue={new Date().toISOString().slice(0, 10)} required />
            </label>
            <label>
              Tipo
              <select name="type" required>
                <option value="curso">Curso</option>
                <option value="treinamento">Treinamento</option>
                <option value="advertencia">Advertencia</option>
                <option value="indisciplina">Indisciplina</option>
                <option value="elogio">Elogio</option>
                <option value="observacao">Observacao</option>
                <option value="mudanca_operacional">Mudanca operacional</option>
                <option value="outro">Outro</option>
              </select>
            </label>
            <label>
              Responsavel
              <input name="responsible" placeholder="Nome do responsavel" required />
            </label>
            <label className="span-2">
              Titulo
              <input name="title" placeholder="Ex: Treinamento de seguranca" required />
            </label>
            <label className="span-2">
              Descricao
              <textarea name="description" placeholder="Descreva o evento registrado" required />
            </label>
            <button type="submit">Registrar evento</button>
            </form>
          )}

          <DataTable
            title="Acompanhamento do colaborador"
            columns={canEdit ? ['Data', 'Tipo', 'Titulo', 'Responsavel', 'Status', 'Descricao', 'Acoes'] : ['Data', 'Tipo', 'Titulo', 'Responsavel', 'Status', 'Descricao']}
            rows={employeeEvents.map((event) => [
              formatDate(event.eventDate),
              labelEmployeeEventType(event.type),
              event.title,
              event.responsible,
              eventStatusBadge(event.status),
              event.description,
              ...(canEdit
                ? [actionButton('Cancelar', () => void handleCancelEmployeeEvent(event.id), event.status === 'cancelado')]
                : []),
            ])}
          />
        </Modal>
      )}

      {editingFixedEntry && (
        <Modal title="Editar lancamento fixo" onClose={() => setEditingFixedEntry(null)}>
          <FixedPayrollEntryEditForm
            entry={editingFixedEntry}
            fixedRubrics={fixedRubrics}
            onCancel={() => setEditingFixedEntry(null)}
            onSubmit={async (event) => {
              await Promise.resolve(onFixedUpdate(editingFixedEntry, event))
              setEditingFixedEntry(null)
            }}
          />
        </Modal>
      )}

      {closingFixedEntry && (
        <Modal title="Encerrar lancamento fixo" onClose={() => setClosingFixedEntry(null)}>
          <FixedPayrollEntryCloseForm
            entry={closingFixedEntry}
            onCancel={() => setClosingFixedEntry(null)}
            onSubmit={async (event) => {
              await Promise.resolve(onFixedClose(closingFixedEntry, event))
              setClosingFixedEntry(null)
            }}
          />
        </Modal>
      )}
    </>
  )
}

function StructureView({
  canEdit,
  activeCompany,
  departments,
  jobPositions,
  onDepartmentSubmit,
  onJobPositionSubmit,
  onToggleDepartmentStatus,
  onToggleJobPositionStatus,
}: {
  canEdit: boolean
  activeCompany: Company | undefined
  departments: Department[]
  jobPositions: JobPosition[]
  onDepartmentSubmit: (event: FormEvent<HTMLFormElement>) => void
  onJobPositionSubmit: (event: FormEvent<HTMLFormElement>) => void
  onToggleDepartmentStatus: (department: Department) => void
  onToggleJobPositionStatus: (jobPosition: JobPosition) => void
}) {
  const [isDepartmentModalOpen, setIsDepartmentModalOpen] = useState(false)
  const [isJobPositionModalOpen, setIsJobPositionModalOpen] = useState(false)

  async function handleDepartmentSubmit(event: FormEvent<HTMLFormElement>) {
    await Promise.resolve(onDepartmentSubmit(event))
    setIsDepartmentModalOpen(false)
  }

  async function handleJobPositionSubmit(event: FormEvent<HTMLFormElement>) {
    await Promise.resolve(onJobPositionSubmit(event))
    setIsJobPositionModalOpen(false)
  }

  return (
    <>
      {canEdit && (
        <section className="section-actions">
          <button type="button" onClick={() => setIsDepartmentModalOpen(true)}>
            Novo setor
          </button>
          <button type="button" onClick={() => setIsJobPositionModalOpen(true)}>
            Novo cargo
          </button>
        </section>
      )}

      {isDepartmentModalOpen && (
        <Modal title="Novo setor" onClose={() => setIsDepartmentModalOpen(false)}>
          <form className="form-grid" onSubmit={(event) => void handleDepartmentSubmit(event)}>
            <label>
              Nome
              <input name="name" placeholder="Ex: Financeiro" required />
            </label>
            <label>
              Codigo
              <input name="internalCode" placeholder="FIN" required />
            </label>
            <div className="modal-actions">
              <button type="button" onClick={() => setIsDepartmentModalOpen(false)}>
                Cancelar
              </button>
              <button type="submit" disabled={!activeCompany}>
                Salvar setor
              </button>
            </div>
          </form>
        </Modal>
      )}

      {isJobPositionModalOpen && (
        <Modal title="Novo cargo" onClose={() => setIsJobPositionModalOpen(false)}>
          <form className="form-grid" onSubmit={(event) => void handleJobPositionSubmit(event)}>
            <label>
              Nome
              <input name="name" placeholder="Ex: Analista de RH" required />
            </label>
            <label>
              Codigo
              <input name="internalCode" placeholder="AN-RH" required />
            </label>
            <label>
              CBO
              <input name="cbo" placeholder="Opcional" />
            </label>
            <div className="modal-actions">
              <button type="button" onClick={() => setIsJobPositionModalOpen(false)}>
                Cancelar
              </button>
              <button type="submit" disabled={!activeCompany}>
                Salvar cargo
              </button>
            </div>
          </form>
        </Modal>
      )}

      <section className="tables-grid">
        <DataTable
          title="Setores cadastrados"
          columns={canEdit ? ['Codigo', 'Nome', 'Status', 'Acoes'] : ['Codigo', 'Nome', 'Status']}
          rows={departments.map((department) => [
            department.internalCode,
            department.name,
            activeBadge(department.isActive),
            ...(canEdit
              ? [actionButton(department.isActive ? 'Inativar' : 'Reativar', () => onToggleDepartmentStatus(department))]
              : []),
          ])}
        />
        <DataTable
          title="Cargos cadastrados"
          columns={canEdit ? ['Codigo', 'Nome', 'CBO', 'Status', 'Acoes'] : ['Codigo', 'Nome', 'CBO', 'Status']}
          rows={jobPositions.map((jobPosition) => [
            jobPosition.internalCode,
            jobPosition.name,
            jobPosition.cbo ?? '-',
            activeBadge(jobPosition.isActive),
            ...(canEdit
              ? [actionButton(jobPosition.isActive ? 'Inativar' : 'Reativar', () => onToggleJobPositionStatus(jobPosition))]
              : []),
          ])}
        />
      </section>
    </>
  )
}

function SettingsView({
  activeCompany,
  companies,
  statutoryTables,
  productionRateTables,
  users,
  auditLogs,
  hrEmployees,
  departments,
  jobPositions,
  dapicIntegrations,
  dapicLogs,
  dapicEmployees,
  dapicProducts,
  dapicOperations,
  dapicCells,
  dapicOrders,
  dapicBusyMessage,
  dapicOrderStartDate,
  dapicOrderEndDate,
  currentUser,
  onSubmit,
  onToggleStatus,
  onDuplicate,
  onProductionRateTableSubmit,
  onProductionRateTableUpdate,
  onProductionRateTableDuplicate,
  onProductionRateTableToggleStatus,
  onUserSubmit,
  onUserUpdate,
  onUserResetPassword,
  onUserToggleStatus,
  onDapicConfigure,
  onDapicTest,
  onDapicSync,
  onDapicEmployeeLink,
  onDapicEmployeeIgnore,
  onDapicEmployeeReset,
  onDapicEmployeeCreateAndLink,
  onDapicOrderStartDateChange,
  onDapicOrderEndDateChange,
}: {
  activeCompany: Company | undefined
  companies: Company[]
  statutoryTables: StatutoryTable[]
  productionRateTables: ProductionRateTable[]
  users: SystemUser[]
  auditLogs: AuditLog[]
  hrEmployees: Employee[]
  departments: Department[]
  jobPositions: JobPosition[]
  dapicIntegrations: DapicIntegration[]
  dapicLogs: DapicSyncLog[]
  dapicEmployees: DapicEmployee[]
  dapicProducts: DapicProduct[]
  dapicOperations: DapicNamedProduction[]
  dapicCells: DapicNamedProduction[]
  dapicOrders: DapicProductionOrder[]
  dapicBusyMessage: string | null
  dapicOrderStartDate: string
  dapicOrderEndDate: string
  currentUser: AuthUser
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onToggleStatus: (table: StatutoryTable) => void
  onDuplicate: (table: StatutoryTable) => void
  onProductionRateTableSubmit: (event: FormEvent<HTMLFormElement>) => Promise<boolean>
  onProductionRateTableUpdate: (table: ProductionRateTable, event: FormEvent<HTMLFormElement>) => Promise<boolean>
  onProductionRateTableDuplicate: (table: ProductionRateTable, event: FormEvent<HTMLFormElement>) => Promise<boolean>
  onProductionRateTableToggleStatus: (table: ProductionRateTable) => void
  onUserSubmit: (event: FormEvent<HTMLFormElement>) => void
  onUserUpdate: (user: SystemUser, event: FormEvent<HTMLFormElement>) => void
  onUserResetPassword: (user: SystemUser, event: FormEvent<HTMLFormElement>) => void
  onUserToggleStatus: (user: SystemUser) => void
  onDapicConfigure: (event: FormEvent<HTMLFormElement>) => void
  onDapicTest: (integration: DapicIntegration) => void
  onDapicSync: (integration: DapicIntegration, resource: string) => void
  onDapicEmployeeLink: (dapicEmployee: DapicEmployee, employeeId: string) => void
  onDapicEmployeeIgnore: (dapicEmployee: DapicEmployee) => void
  onDapicEmployeeReset: (dapicEmployee: DapicEmployee) => void
  onDapicEmployeeCreateAndLink: (dapicEmployee: DapicEmployee, payload: CreateLinkedEmployeePayload) => void
  onDapicOrderStartDateChange: (value: string) => void
  onDapicOrderEndDateChange: (value: string) => void
}) {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isProductionRateModalOpen, setIsProductionRateModalOpen] = useState(false)
  const [viewingProductionRateTable, setViewingProductionRateTable] = useState<ProductionRateTable | null>(null)
  const [editingProductionRateTable, setEditingProductionRateTable] = useState<ProductionRateTable | null>(null)
  const [duplicatingProductionRateTable, setDuplicatingProductionRateTable] = useState<ProductionRateTable | null>(null)
  const [isUserModalOpen, setIsUserModalOpen] = useState(false)
  const [editingUser, setEditingUser] = useState<SystemUser | null>(null)
  const [resetPasswordUser, setResetPasswordUser] = useState<SystemUser | null>(null)
  const dapicIntegration = dapicIntegrations[0] ?? null
  const [dapicConferenceTab, setDapicConferenceTab] = useState<DapicConferenceTab>('employees')
  const [dapicConferenceSearch, setDapicConferenceSearch] = useState('')
  const [dapicConferenceStatus, setDapicConferenceStatus] = useState('all')

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    await Promise.resolve(onSubmit(event))
    setIsCreateModalOpen(false)
  }

  async function handleProductionRateSubmit(event: FormEvent<HTMLFormElement>) {
    const saved = await onProductionRateTableSubmit(event)
    if (saved) {
      setIsProductionRateModalOpen(false)
    }
  }

  async function handleProductionRateUpdate(event: FormEvent<HTMLFormElement>) {
    if (!editingProductionRateTable) return

    const saved = await onProductionRateTableUpdate(editingProductionRateTable, event)
    if (saved) {
      setEditingProductionRateTable(null)
    }
  }

  async function handleProductionRateDuplicate(event: FormEvent<HTMLFormElement>) {
    if (!duplicatingProductionRateTable) return

    const saved = await onProductionRateTableDuplicate(duplicatingProductionRateTable, event)
    if (saved) {
      setDuplicatingProductionRateTable(null)
    }
  }

  return (
    <>
      <section className="section-actions">
        <button type="button" onClick={() => setIsUserModalOpen(true)}>
          Novo usuario
        </button>
        <button type="button" onClick={() => setIsCreateModalOpen(true)}>
          Nova tabela legal
        </button>
      </section>

      <Panel title="Empresa">
        <div className="company-list">
          {companies.map((company) => (
            <article key={company.id}>
              <span>{company.tradeName ?? company.legalName}</span>
              <strong>{company.documentNumber}</strong>
            </article>
          ))}
        </div>
      </Panel>

      <Panel title="Integracao Dapic" className="dapic-integration-panel">
        {dapicBusyMessage && (
          <div className="integration-progress" role="status" aria-live="polite">
            <span className="loading-spinner" />
            <strong>{dapicBusyMessage}</strong>
          </div>
        )}

        <div className="integration-layout">
          <form
            key={dapicIntegration?.id ?? 'new-dapic'}
            className="entry-form"
            onSubmit={(event) => onDapicConfigure(event)}
          >
            <label>
              Nome
              <input name="name" defaultValue={dapicIntegration?.name ?? 'Dapic Webpic'} required />
            </label>
            <label>
              URL base
              <input name="baseUrl" defaultValue={dapicIntegration?.baseUrl ?? 'https://api.dapic.app'} required />
            </label>
            <label>
              Empresa Dapic
              <input
                name="externalCompanyIdentifier"
                defaultValue={dapicIntegration?.externalCompanyIdentifier ?? ''}
                placeholder="Identificador informado pela Dapic"
                required
              />
            </label>
            <label className="span-2">
              Token de integracao
              <input
                name="integrationToken"
                type="password"
                placeholder={dapicIntegration ? 'Informe o token para salvar a configuracao' : 'TokenIntegracao'}
                required
              />
            </label>
            <button type="submit" disabled={!activeCompany || Boolean(dapicBusyMessage)}>
              Salvar configuracao
            </button>
          </form>

          <div className="integration-summary">
            <div>
              <span>Status</span>
              <strong>{dapicIntegration ? labelDapicStatus(dapicIntegration.status) : 'Nao configurada'}</strong>
            </div>
            <div>
              <span>Ultima sincronizacao</span>
              <strong>{dapicIntegration?.lastSyncAt ? formatDateTime(dapicIntegration.lastSyncAt) : '-'}</strong>
            </div>
            <div>
              <span>Token valido ate</span>
              <strong>
                {dapicIntegration?.accessTokenExpiresAt ? formatDateTime(dapicIntegration.accessTokenExpiresAt) : '-'}
              </strong>
            </div>
            {dapicIntegration?.lastError && (
              <div className="integration-error">
                <span>Ultimo erro</span>
                <strong>{dapicIntegration.lastError}</strong>
              </div>
            )}
          </div>
        </div>

        <div className="quick-actions integration-actions">
          <button type="button" disabled={!dapicIntegration || Boolean(dapicBusyMessage)} onClick={() => dapicIntegration && onDapicTest(dapicIntegration)}>
            Testar conexao
          </button>
          <button type="button" disabled={!dapicIntegration || Boolean(dapicBusyMessage)} onClick={() => dapicIntegration && onDapicSync(dapicIntegration, 'employees')}>
            Sincronizar funcionarios
          </button>
          <button type="button" disabled={!dapicIntegration || Boolean(dapicBusyMessage)} onClick={() => dapicIntegration && onDapicSync(dapicIntegration, 'products')}>
            Sincronizar produtos
          </button>
          <button type="button" disabled={!dapicIntegration || Boolean(dapicBusyMessage)} onClick={() => dapicIntegration && onDapicSync(dapicIntegration, 'operations')}>
            Sincronizar operacoes
          </button>
          <button type="button" disabled={!dapicIntegration || Boolean(dapicBusyMessage)} onClick={() => dapicIntegration && onDapicSync(dapicIntegration, 'cells')}>
            Sincronizar celulas
          </button>
        </div>

        <div className="integration-order-sync">
          <label>
            Data inicial das ordens
            <input
              type="date"
              value={dapicOrderStartDate}
              onChange={(event) => onDapicOrderStartDateChange(event.currentTarget.value)}
              max={todayInputDate()}
              required
            />
          </label>
          <label>
            Data final das ordens
            <input
              type="date"
              value={dapicOrderEndDate}
              onChange={(event) => onDapicOrderEndDateChange(event.currentTarget.value)}
              max={todayInputDate()}
              required
            />
          </label>
          <button type="button" disabled={!dapicIntegration || Boolean(dapicBusyMessage)} onClick={() => dapicIntegration && onDapicSync(dapicIntegration, 'orders')}>
            Sincronizar ordens
          </button>
          <span>A Dapic exige periodo fechado, sem data final futura.</span>
        </div>
      </Panel>

      <section className="metrics-grid">
        <Metric label="Dapic funcionarios" value={dapicEmployees.length} />
        <Metric label="Dapic produtos" value={dapicProducts.length} />
        <Metric label="Dapic operacoes" value={dapicOperations.length} />
        <Metric label="Dapic celulas" value={dapicCells.length} />
        <Metric label="Dapic ordens" value={dapicOrders.length} />
      </section>

      <DapicConferencePanel
        activeTab={dapicConferenceTab}
        search={dapicConferenceSearch}
        status={dapicConferenceStatus}
        employees={hrEmployees}
        departments={departments}
        jobPositions={jobPositions}
        dapicEmployees={dapicEmployees}
        products={dapicProducts}
        operations={dapicOperations}
        cells={dapicCells}
        orders={dapicOrders}
        onTabChange={setDapicConferenceTab}
        onSearchChange={setDapicConferenceSearch}
        onStatusChange={setDapicConferenceStatus}
        onEmployeeLink={onDapicEmployeeLink}
        onEmployeeIgnore={onDapicEmployeeIgnore}
        onEmployeeReset={onDapicEmployeeReset}
        onEmployeeCreateAndLink={onDapicEmployeeCreateAndLink}
      />

      <DataTable
        title="Ultimas sincronizacoes Dapic"
        columns={['Inicio', 'Recurso', 'Status', 'Paginas', 'Lidos', 'Criados', 'Atualizados', 'Erro']}
        pageSize={10}
        rows={dapicLogs.map((log) => [
          formatDateTime(log.startedAt),
          labelDapicResource(log.resource),
          labelDapicStatus(log.status),
          String(log.pageCount),
          String(log.recordsRead),
          String(log.recordsCreated),
          String(log.recordsUpdated),
          log.errorMessage ?? '-',
        ])}
      />

      {isCreateModalOpen && (
        <Modal title="Nova tabela legal" onClose={() => setIsCreateModalOpen(false)} size="wide">
          <form className="statutory-table-form" onSubmit={(event) => void handleSubmit(event)}>
            <label>
              Tipo
              <select name="type" required>
                <option value="inss">INSS</option>
                <option value="irrf">IRRF</option>
                <option value="irrf_reducao">IRRF reducao</option>
                <option value="fgts">FGTS</option>
                <option value="fgts_saque_aniversario">FGTS saque-aniversario</option>
              </select>
            </label>
            <label>
              Nome
              <input name="name" placeholder="Ex: INSS 2026" required />
            </label>
            <label>
              Inicio vigencia
              <input name="startsOn" type="date" defaultValue="2026-01-01" required />
            </label>
            <label>
              Fim vigencia
              <input name="endsOn" type="date" />
            </label>
            <label className="span-2">
              Observacao
              <textarea
                name="notes"
                rows={3}
                defaultValue="Valores parametrizados para desenvolvimento. Revisar tabela oficial antes de producao."
              />
            </label>

            <div className="range-editor span-2">
              <div className="range-editor-header">
                <span>Faixa</span>
                <span>De</span>
                <span>Ate</span>
                <span>Aliquota %</span>
                <span>Deduzir</span>
              </div>
              {[1, 2, 3, 4, 5].map((index) => (
                <div className="range-editor-row" key={index}>
                  <strong>{index}</strong>
                  <input name={`lowerLimit${index}`} type="number" min="0" step="0.01" defaultValue={index === 1 ? 0 : ''} />
                  <input name={`upperLimit${index}`} type="number" min="0" step="0.01" />
                  <input name={`ratePercent${index}`} type="number" min="0" step="0.0001" />
                  <input name={`deductionAmount${index}`} type="number" min="0" step="0.01" defaultValue="0" />
                </div>
              ))}
            </div>

            <div className="modal-actions">
              <button type="button" onClick={() => setIsCreateModalOpen(false)}>
                Cancelar
              </button>
              <button type="submit" disabled={!activeCompany}>
                Salvar tabela
              </button>
            </div>
          </form>
        </Modal>
      )}

      {isProductionRateModalOpen && (
        <Modal title="Nova tabela de producao" onClose={() => setIsProductionRateModalOpen(false)} size="wide">
          <ProductionRateTableCreateForm
            activeCompany={activeCompany}
            products={dapicProducts}
            operations={dapicOperations}
            cells={dapicCells}
            departments={departments}
            jobPositions={jobPositions}
            onCancel={() => setIsProductionRateModalOpen(false)}
            onSubmit={handleProductionRateSubmit}
          />
        </Modal>
      )}

      {viewingProductionRateTable && (
        <Modal title="Detalhes da tabela de producao" onClose={() => setViewingProductionRateTable(null)} size="wide">
          <ProductionRateTableDetails table={viewingProductionRateTable} />
        </Modal>
      )}

      {editingProductionRateTable && (
        <Modal title="Editar tabela de producao" onClose={() => setEditingProductionRateTable(null)} size="wide">
          <ProductionRateTableCreateForm
            activeCompany={activeCompany}
            products={dapicProducts}
            operations={dapicOperations}
            cells={dapicCells}
            departments={departments}
            jobPositions={jobPositions}
            initialTable={editingProductionRateTable}
            submitLabel="Salvar alteracoes"
            onCancel={() => setEditingProductionRateTable(null)}
            onSubmit={handleProductionRateUpdate}
          />
        </Modal>
      )}

      {duplicatingProductionRateTable && (
        <Modal title="Nova vigencia de producao" onClose={() => setDuplicatingProductionRateTable(null)}>
          <ProductionRateTableDuplicateForm
            table={duplicatingProductionRateTable}
            onCancel={() => setDuplicatingProductionRateTable(null)}
            onSubmit={handleProductionRateDuplicate}
          />
        </Modal>
      )}

      {isUserModalOpen && (
        <Modal title="Novo usuario" onClose={() => setIsUserModalOpen(false)}>
          <UserCreateForm
            companies={companies}
            onCancel={() => setIsUserModalOpen(false)}
            onSubmit={async (event) => {
              await Promise.resolve(onUserSubmit(event))
              setIsUserModalOpen(false)
            }}
          />
        </Modal>
      )}

      {editingUser && (
        <Modal title="Editar usuario" onClose={() => setEditingUser(null)}>
          <UserEditForm
            user={editingUser}
            companies={companies}
            onCancel={() => setEditingUser(null)}
            onSubmit={async (event) => {
              await Promise.resolve(onUserUpdate(editingUser, event))
              setEditingUser(null)
            }}
          />
        </Modal>
      )}

      {resetPasswordUser && (
        <Modal title="Redefinir senha" onClose={() => setResetPasswordUser(null)}>
          <UserResetPasswordForm
            user={resetPasswordUser}
            onCancel={() => setResetPasswordUser(null)}
            onSubmit={async (event) => {
              await Promise.resolve(onUserResetPassword(resetPasswordUser, event))
              setResetPasswordUser(null)
            }}
          />
        </Modal>
      )}

      <DataTable
        title="Usuarios e permissoes"
        columns={['Nome', 'E-mail', 'Empresa', 'Perfil', 'Status', 'Ultimo login', 'Acoes']}
        pageSize={10}
        rows={users.map((user) => [
          user.fullName,
          user.email,
          companies.find((company) => company.id === user.companyId)?.tradeName ??
            companies.find((company) => company.id === user.companyId)?.legalName ??
            'Todas',
          labelUserRole(user.role),
          activeBadge(user.isActive),
          user.lastLoginAt ? formatDateTime(user.lastLoginAt) : '-',
          (
            <div className="table-actions">
              {actionButton('Editar', () => setEditingUser(user))}
              {actionButton('Senha', () => setResetPasswordUser(user))}
              {actionButton(
                user.isActive ? 'Inativar' : 'Reativar',
                () => onUserToggleStatus(user),
                user.id === currentUser.id,
              )}
            </div>
          ),
        ])}
      />

      <DataTable
        title="Auditoria recente"
        columns={['Data', 'Usuario', 'Perfil', 'Acao', 'Entidade', 'Descricao', 'IP']}
        pageSize={10}
        rows={auditLogs.map((log) => [
          formatDateTime(log.createdAt),
          `${log.userName} (${log.userEmail})`,
          labelUserRole(log.userRole),
          labelAuditAction(log.action),
          log.entityId ? `${log.entityName} ${log.entityId.slice(0, 8)}` : log.entityName,
          log.description,
          log.ipAddress ?? '-',
        ])}
      />

      <DataTable
        title="Tabelas de valores de producao"
        action={
          <button type="button" onClick={() => setIsProductionRateModalOpen(true)}>
            Nova tabela de producao
          </button>
        }
        columns={['Nome', 'Vigencia', 'Status', 'Regras', 'Acoes']}
        pageSize={10}
        rows={productionRateTables.map((table) => [
          table.name,
          `${formatDate(table.effectiveFrom)} ate ${table.effectiveTo ? formatDate(table.effectiveTo) : 'vigente'}`,
          productionRateStatusBadge(table.status),
          formatProductionRateRules(table.rates),
          (
            <div className="table-actions">
              {actionButton('Detalhes', () => setViewingProductionRateTable(table))}
              {actionButton('Editar', () => setEditingProductionRateTable(table), table.status !== 'Draft')}
              {actionButton('Nova vigencia', () => setDuplicatingProductionRateTable(table))}
              {actionButton(table.status === 'Active' ? 'Inativar' : 'Ativar', () => onProductionRateTableToggleStatus(table))}
            </div>
          ),
        ])}
      />

      <DataTable
        title="Tabelas legais parametrizadas"
        columns={['Tipo', 'Nome', 'Vigencia', 'Faixas', 'Status', 'Acoes']}
        pageSize={10}
        rows={statutoryTables.map((table) => [
          labelStatutoryTableType(table.type),
          table.name,
          `${formatDate(table.startsOn)} ate ${table.endsOn ? formatDate(table.endsOn) : 'vigente'}`,
          formatStatutoryRanges(table.ranges),
          activeBadge(table.isActive),
          (
            <div className="table-actions">
              {actionButton('Nova vigencia', () => onDuplicate(table))}
              {actionButton(table.isActive ? 'Inativar' : 'Reativar', () => onToggleStatus(table))}
            </div>
          ),
        ])}
      />
    </>
  )
}

function ProductionRateTableCreateForm({
  activeCompany,
  products,
  operations,
  cells,
  departments,
  jobPositions,
  initialTable,
  submitLabel = 'Salvar tabela',
  onCancel,
  onSubmit,
}: {
  activeCompany: Company | undefined
  products: DapicProduct[]
  operations: DapicNamedProduction[]
  cells: DapicNamedProduction[]
  departments: Department[]
  jobPositions: JobPosition[]
  initialTable?: ProductionRateTable
  submitLabel?: string
  onCancel: () => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}) {
  const productOptions = products
    .slice()
    .sort((first, second) => first.reference.localeCompare(second.reference))
    .slice(0, 300)
  const rowCount = Math.max(5, initialTable?.rates.length ?? 0)
  const rows = Array.from({ length: rowCount }, (_, index) => index + 1)

  return (
    <form className="statutory-table-form" onSubmit={(event) => void onSubmit(event)}>
      <label>
        Nome
        <input name="name" placeholder="Ex: Producao junho 2026" defaultValue={initialTable?.name ?? ''} required />
      </label>
      <label>
        Inicio vigencia
        <input name="effectiveFrom" type="date" defaultValue={initialTable?.effectiveFrom ?? currentMonthStart()} required />
      </label>
      <label>
        Fim vigencia
        <input name="effectiveTo" type="date" defaultValue={initialTable?.effectiveTo ?? ''} />
      </label>
      <label className="span-2">
        Observacao
        <textarea name="notes" rows={3} placeholder="Ex: valores negociados por operacao e celula" defaultValue={initialTable?.notes ?? ''} />
      </label>

      <div className="rate-editor span-2">
        {rows.map((index) => {
          const rate = initialTable?.rates[index - 1]

          return (
          <div className="rate-editor-row" key={index}>
            <strong>Regra {index}</strong>
            <label>
              Produto
              <select name={`productionProductId${index}`} defaultValue={rate?.productionProductId ?? ''}>
                <option value="">Todos</option>
                {productOptions.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.reference} - {product.factoryDescription}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Operacao
              <select name={`productionOperationId${index}`} defaultValue={rate?.productionOperationId ?? ''}>
                <option value="">Todas</option>
                {operations.map((operation) => (
                  <option key={operation.id} value={operation.id}>
                    {operation.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Celula
              <select name={`productionCellId${index}`} defaultValue={rate?.productionCellId ?? ''}>
                <option value="">Todas</option>
                {cells.map((cell) => (
                  <option key={cell.id} value={cell.id}>
                    {cell.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Setor
              <select name={`productionDepartmentId${index}`} defaultValue={rate?.departmentId ?? ''}>
                <option value="">Todos</option>
                {departments.map((department) => (
                  <option key={department.id} value={department.id}>
                    {department.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Cargo
              <select name={`productionJobPositionId${index}`} defaultValue={rate?.jobPositionId ?? ''}>
                <option value="">Todos</option>
                {jobPositions.map((jobPosition) => (
                  <option key={jobPosition.id} value={jobPosition.id}>
                    {jobPosition.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Valor
              <input name={`productionUnitValue${index}`} type="number" min="0" step="0.01" placeholder="0,00" defaultValue={rate?.unitValue ?? ''} />
            </label>
            <label>
              Qtd min.
              <input name={`productionMinimumQuantity${index}`} type="number" min="0" step="0.0001" defaultValue={rate?.minimumQuantity ?? ''} />
            </label>
            <label>
              Qtd max.
              <input name={`productionMaximumQuantity${index}`} type="number" min="0" step="0.0001" defaultValue={rate?.maximumQuantity ?? ''} />
            </label>
            <label>
              Obs.
              <input name={`productionRateNotes${index}`} placeholder="Opcional" defaultValue={rate?.notes ?? ''} />
            </label>
          </div>
          )
        })}
      </div>

      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit" disabled={!activeCompany}>
          {submitLabel}
        </button>
      </div>
    </form>
  )
}

function ProductionRateTableDetails({ table }: { table: ProductionRateTable }) {
  return (
    <div className="production-rate-details">
      <div className="detail-grid">
        <div>
          <span>Nome</span>
          <strong>{table.name}</strong>
        </div>
        <div>
          <span>Vigencia</span>
          <strong>{formatDate(table.effectiveFrom)} ate {table.effectiveTo ? formatDate(table.effectiveTo) : 'vigente'}</strong>
        </div>
        <div>
          <span>Status</span>
          <strong>{labelProductionRateStatus(table.status)}</strong>
        </div>
        <div>
          <span>Observacao</span>
          <strong>{table.notes ?? '-'}</strong>
        </div>
      </div>

      <div className="production-rate-rule-list">
        {table.rates.map((rate, index) => (
          <article key={rate.id}>
            <header>
              <strong>Regra {index + 1}</strong>
              <span>{formatCurrency(rate.unitValue)}</span>
            </header>
            <dl>
              <div>
                <dt>Produto</dt>
                <dd>{rate.productReference ? `${rate.productReference} - ${rate.productDescription ?? ''}` : 'Todos'}</dd>
              </div>
              <div>
                <dt>Operacao</dt>
                <dd>{rate.operationName ?? 'Todas'}</dd>
              </div>
              <div>
                <dt>Celula</dt>
                <dd>{rate.cellName ?? 'Todas'}</dd>
              </div>
              <div>
                <dt>Setor</dt>
                <dd>{rate.departmentName ?? 'Todos'}</dd>
              </div>
              <div>
                <dt>Cargo</dt>
                <dd>{rate.jobPositionName ?? 'Todos'}</dd>
              </div>
              <div>
                <dt>Quantidade</dt>
                <dd>{formatProductionQuantityRange(rate)}</dd>
              </div>
              <div>
                <dt>Observacao</dt>
                <dd>{rate.notes ?? '-'}</dd>
              </div>
            </dl>
          </article>
        ))}
      </div>
    </div>
  )
}

function ProductionRateTableDuplicateForm({
  table,
  onCancel,
  onSubmit,
}: {
  table: ProductionRateTable
  onCancel: () => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}) {
  return (
    <form className="entry-form" onSubmit={(event) => void onSubmit(event)}>
      <label className="span-2">
        Nome
        <input name="name" defaultValue={`${table.name} - nova vigencia`} required />
      </label>
      <label>
        Inicio vigencia
        <input name="effectiveFrom" type="date" defaultValue={nextMonthStart(table.effectiveFrom)} required />
      </label>
      <label>
        Fim vigencia
        <input name="effectiveTo" type="date" />
      </label>
      <label className="span-2">
        Observacao
        <textarea name="notes" rows={3} defaultValue={table.notes ?? ''} />
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit">
          Criar rascunho
        </button>
      </div>
    </form>
  )
}

function NavButton({ active, onClick, children }: { active: boolean; onClick: () => void; children: ReactNode }) {
  return (
    <button type="button" className={active ? 'nav-item active' : 'nav-item'} onClick={onClick}>
      {children}
    </button>
  )
}

function DapicConferencePanel({
  activeTab,
  search,
  status,
  employees,
  departments,
  jobPositions,
  dapicEmployees,
  products,
  operations,
  cells,
  orders,
  onTabChange,
  onSearchChange,
  onStatusChange,
  onEmployeeLink,
  onEmployeeIgnore,
  onEmployeeReset,
  onEmployeeCreateAndLink,
}: {
  activeTab: DapicConferenceTab
  search: string
  status: string
  employees: Employee[]
  departments: Department[]
  jobPositions: JobPosition[]
  dapicEmployees: DapicEmployee[]
  products: DapicProduct[]
  operations: DapicNamedProduction[]
  cells: DapicNamedProduction[]
  orders: DapicProductionOrder[]
  onTabChange: (tab: DapicConferenceTab) => void
  onSearchChange: (value: string) => void
  onStatusChange: (value: string) => void
  onEmployeeLink: (dapicEmployee: DapicEmployee, employeeId: string) => void
  onEmployeeIgnore: (dapicEmployee: DapicEmployee) => void
  onEmployeeReset: (dapicEmployee: DapicEmployee) => void
  onEmployeeCreateAndLink: (dapicEmployee: DapicEmployee, payload: CreateLinkedEmployeePayload) => void
}) {
  const [page, setPage] = useState(1)
  const [linkingEmployee, setLinkingEmployee] = useState<DapicEmployee | null>(null)
  const [creatingEmployee, setCreatingEmployee] = useState<DapicEmployee | null>(null)
  const pageSize = 15
  const employeeSuggestions = useMemo(() => buildDapicEmployeeSuggestions(dapicEmployees, employees), [dapicEmployees, employees])
  const tabs: Array<{ id: DapicConferenceTab; label: string; count: number }> = [
    { id: 'employees', label: 'Funcionarios', count: dapicEmployees.length },
    { id: 'products', label: 'Produtos', count: products.length },
    { id: 'operations', label: 'Operacoes', count: operations.length },
    { id: 'cells', label: 'Celulas', count: cells.length },
    { id: 'orders', label: 'Ordens', count: orders.length },
  ]
  const current = buildDapicConferenceRows(
    activeTab,
    { employees: dapicEmployees, products, operations, cells, orders },
    {
      employeeSuggestions,
      onEmployeeLink: setLinkingEmployee,
      onEmployeeCreate: setCreatingEmployee,
      onEmployeeSuggestionLink: onEmployeeLink,
      onEmployeeIgnore,
      onEmployeeReset,
    },
  )
  const statuses = Array.from(new Set(current.records.map((record) => record.status).filter(Boolean))).sort((left, right) =>
    labelDapicStatus(left).localeCompare(labelDapicStatus(right), 'pt-BR'),
  )
  const normalizedSearch = normalizeSearch(search)
  const filtered = current.records.filter((record) => {
    const statusMatches = status === 'all' || record.status === status
    const searchMatches = !normalizedSearch || normalizeSearch(record.searchText).includes(normalizedSearch)
    return statusMatches && searchMatches
  })
  const totalPages = Math.max(Math.ceil(filtered.length / pageSize), 1)
  const safePage = Math.min(page, totalPages)
  const firstVisibleIndex = filtered.length === 0 ? 0 : (safePage - 1) * pageSize + 1
  const lastVisibleIndex = Math.min(safePage * pageSize, filtered.length)
  const visible = filtered.slice((safePage - 1) * pageSize, safePage * pageSize)

  useEffect(() => {
    setPage(1)
  }, [activeTab, search, status])

  return (
    <section className="panel dapic-conference">
      <div className="dapic-conference-header">
        <div>
          <h3>Conferencia da importacao Dapic</h3>
          <span>
            {filtered.length} de {current.records.length} registro(s) encontrados.
            {filtered.length > 0 && ` Exibindo ${firstVisibleIndex}-${lastVisibleIndex}.`}
          </span>
        </div>
        <div className="origin-legend">
          {originBadge('manual')}
          {originBadge('dapic')}
        </div>
      </div>

      <div className="tab-list" role="tablist" aria-label="Dados importados da Dapic">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            className={activeTab === tab.id ? 'tab-button active' : 'tab-button'}
            onClick={() => {
              onTabChange(tab.id)
              onSearchChange('')
              onStatusChange('all')
            }}
          >
            {tab.label}
            <span>{tab.count}</span>
          </button>
        ))}
      </div>

      <div className="dapic-filter-form">
        <label>
          Pesquisar
          <input
            value={search}
            onChange={(event) => onSearchChange(event.currentTarget.value)}
            placeholder="Nome, referencia, numero, descricao..."
          />
        </label>
        <label>
          Status
          <select value={status} onChange={(event) => onStatusChange(event.currentTarget.value)}>
            <option value="all">Todos</option>
            {statuses.map((statusOption) => (
              <option key={statusOption} value={statusOption}>
                {activeTab === 'employees' ? labelDapicLinkStatus(statusOption) : labelDapicStatus(statusOption)}
              </option>
            ))}
          </select>
        </label>
      </div>

      <PaginationControls
        page={safePage}
        totalPages={totalPages}
        pageSize={pageSize}
        totalRecords={filtered.length}
        onFirst={() => setPage(1)}
        onPrevious={() => setPage((currentPage) => Math.max(currentPage - 1, 1))}
        onNext={() => setPage((currentPage) => Math.min(currentPage + 1, totalPages))}
        onLast={() => setPage(totalPages)}
      />

      <div className="table-scroll">
        <table>
          <thead>
            <tr>
              {current.columns.map((column) => (
                <th key={column}>{column}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {visible.map((record) => (
              <tr key={record.key}>
                {record.cells.map((cell, index) => (
                  <td key={`${record.key}-${index}`}>{cell}</td>
                ))}
              </tr>
            ))}
            {visible.length === 0 && (
              <tr>
                <td colSpan={current.columns.length}>Nenhum registro encontrado para os filtros informados.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <PaginationControls
          page={safePage}
          totalPages={totalPages}
          pageSize={pageSize}
          totalRecords={filtered.length}
          onFirst={() => setPage(1)}
          onPrevious={() => setPage((currentPage) => Math.max(currentPage - 1, 1))}
          onNext={() => setPage((currentPage) => Math.min(currentPage + 1, totalPages))}
          onLast={() => setPage(totalPages)}
        />
      )}

      {linkingEmployee && (
        <Modal title="Vincular funcionario Dapic" onClose={() => setLinkingEmployee(null)}>
          <DapicEmployeeLinkForm
            dapicEmployee={linkingEmployee}
            employees={employees}
            onCancel={() => setLinkingEmployee(null)}
            onSubmit={(employeeId) => {
              onEmployeeLink(linkingEmployee, employeeId)
              setLinkingEmployee(null)
            }}
          />
        </Modal>
      )}

      {creatingEmployee && (
        <Modal title="Criar colaborador e vincular" onClose={() => setCreatingEmployee(null)} size="wide">
          <DapicEmployeeCreateForm
            dapicEmployee={creatingEmployee}
            departments={departments}
            jobPositions={jobPositions}
            onCancel={() => setCreatingEmployee(null)}
            onSubmit={(payload) => {
              onEmployeeCreateAndLink(creatingEmployee, payload)
              setCreatingEmployee(null)
            }}
          />
        </Modal>
      )}
    </section>
  )
}

function buildDapicConferenceRows(
  tab: DapicConferenceTab,
  data: {
    employees: DapicEmployee[]
    products: DapicProduct[]
    operations: DapicNamedProduction[]
    cells: DapicNamedProduction[]
    orders: DapicProductionOrder[]
  },
  actions: {
    employeeSuggestions: Map<string, Employee[]>
    onEmployeeLink: (employee: DapicEmployee) => void
    onEmployeeCreate: (employee: DapicEmployee) => void
    onEmployeeSuggestionLink: (dapicEmployee: DapicEmployee, employeeId: string) => void
    onEmployeeIgnore: (employee: DapicEmployee) => void
    onEmployeeReset: (employee: DapicEmployee) => void
  },
) {
  if (tab === 'employees') {
    return {
      columns: ['Origem', 'Nome', 'Acoes', 'Situacao', 'Sugestao', 'Colaborador vinculado', 'Exibicao', 'Status', 'Ultima sincronizacao'],
      records: data.employees.map((employee) => {
        const suggestions = actions.employeeSuggestions.get(employee.id) ?? []
        return {
          key: employee.id,
          status: employee.linkStatus,
          searchText: [
            employee.externalId,
            employee.name,
            employee.fantasyName,
            employee.displayName,
            employee.status,
            employee.linkStatus,
            employee.employeeRegistration,
            employee.employeeName,
            ...suggestions.map((suggestion) => `${suggestion.registration} ${suggestion.name}`),
          ].join(' '),
          cells: [
            originBadge('dapic'),
            employee.name,
            dapicEmployeeActions(
              employee,
              suggestions,
              actions.onEmployeeLink,
              actions.onEmployeeCreate,
              actions.onEmployeeSuggestionLink,
              actions.onEmployeeIgnore,
              actions.onEmployeeReset,
            ),
            dapicLinkStatusBadge(employee.linkStatus),
            formatDapicEmployeeSuggestion(suggestions),
            employee.employeeName ? `${employee.employeeRegistration ?? '-'} - ${employee.employeeName}` : '-',
            employee.displayName ?? employee.fantasyName ?? '-',
            dapicStatusBadge(employee.status),
            formatDateTime(employee.lastSyncedAt),
          ],
        }
      }),
    }
  }

  if (tab === 'products') {
    return {
      columns: ['Origem', 'Referencia', 'Descricao', 'Status', 'Ultima sincronizacao'],
      records: data.products.map((product) => ({
        key: product.id,
        status: product.status,
        searchText: [product.externalId, product.reference, product.factoryDescription, product.status].join(' '),
        cells: [
          originBadge('dapic'),
          product.reference,
          product.factoryDescription,
          dapicStatusBadge(product.status),
          formatDateTime(product.lastSyncedAt),
        ],
      })),
    }
  }

  if (tab === 'operations') {
    return {
      columns: ['Origem', 'Operacao', 'Descricao', 'Status', 'Ultima sincronizacao'],
      records: data.operations.map((operation) => ({
        key: operation.id,
        status: operation.status,
        searchText: [operation.externalId, operation.name, operation.description, operation.status].join(' '),
        cells: [
          originBadge('dapic'),
          operation.name,
          operation.description ?? '-',
          dapicStatusBadge(operation.status),
          formatDateTime(operation.lastSyncedAt),
        ],
      })),
    }
  }

  if (tab === 'cells') {
    return {
      columns: ['Origem', 'Celula', 'Descricao', 'Status', 'Ultima sincronizacao'],
      records: data.cells.map((cell) => ({
        key: cell.id,
        status: cell.status,
        searchText: [cell.externalId, cell.name, cell.description, cell.status].join(' '),
        cells: [
          originBadge('dapic'),
          cell.name,
          cell.description ?? '-',
          dapicStatusBadge(cell.status),
          formatDateTime(cell.lastSyncedAt),
        ],
      })),
    }
  }

  return {
    columns: ['Origem', 'Numero', 'Descricao', 'Status', 'Data conta', 'Inicio', 'Fim', 'Ultima sincronizacao'],
    records: data.orders.map((order) => ({
      key: order.id,
      status: order.rawStatus ?? order.status,
      searchText: [order.externalId, order.number, order.description, order.status, order.rawStatus].join(' '),
      cells: [
        originBadge('dapic'),
        order.number ?? '-',
        order.description ?? '-',
        dapicStatusBadge(order.rawStatus ?? order.status),
        order.issueDate ? formatDate(order.issueDate) : '-',
        order.startDate ? formatDate(order.startDate) : '-',
        order.endDate ? formatDate(order.endDate) : '-',
        formatDateTime(order.lastSyncedAt),
      ],
    })),
  }
}

function dapicEmployeeActions(
  employee: DapicEmployee,
  suggestions: Employee[],
  onLink: (employee: DapicEmployee) => void,
  onCreate: (employee: DapicEmployee) => void,
  onSuggestionLink: (dapicEmployee: DapicEmployee, employeeId: string) => void,
  onIgnore: (employee: DapicEmployee) => void,
  onReset: (employee: DapicEmployee) => void,
) {
  const singleSuggestion = suggestions.length === 1 ? suggestions[0] : null

  return (
    <div className="table-actions">
      {employee.linkStatus !== 'vinculado' &&
        singleSuggestion &&
        actionButton('Vincular sugestao', () => onSuggestionLink(employee, singleSuggestion.id))}
      {actionButton(employee.linkStatus === 'vinculado' ? 'Alterar vinculo' : 'Vincular', () => onLink(employee))}
      {employee.linkStatus !== 'vinculado' && actionButton('Criar colaborador', () => onCreate(employee))}
      {employee.linkStatus !== 'ignorado' && actionButton('Ignorar', () => onIgnore(employee))}
      {employee.linkStatus !== 'pendente' && actionButton('Redefinir', () => onReset(employee))}
    </div>
  )
}

function formatDapicEmployeeSuggestion(suggestions: Employee[]) {
  if (suggestions.length === 0) {
    return '-'
  }

  if (suggestions.length === 1) {
    const suggestion = suggestions[0]
    return `${suggestion.registration} - ${suggestion.name}`
  }

  return `${suggestions.length} possiveis vinculos`
}

function buildDapicEmployeeSuggestions(dapicEmployees: DapicEmployee[], employees: Employee[]) {
  const employeesByNormalizedName = employees.reduce((map, employee) => {
    const normalized = normalizePersonName(employee.name)
    if (!normalized) return map

    const list = map.get(normalized) ?? []
    list.push(employee)
    map.set(normalized, list)
    return map
  }, new Map<string, Employee[]>())

  return dapicEmployees.reduce((map, dapicEmployee) => {
    if (dapicEmployee.linkStatus !== 'pendente') {
      map.set(dapicEmployee.id, [])
      return map
    }

    const candidates = [
      ...findEmployeesByDapicName(dapicEmployee.name, employeesByNormalizedName),
      ...findEmployeesByDapicName(dapicEmployee.displayName, employeesByNormalizedName),
      ...findEmployeesByDapicName(dapicEmployee.fantasyName, employeesByNormalizedName),
    ]
    const uniqueCandidates = Array.from(new Map(candidates.map((candidate) => [candidate.id, candidate])).values())

    map.set(dapicEmployee.id, uniqueCandidates)
    return map
  }, new Map<string, Employee[]>())
}

function findEmployeesByDapicName(value: string | null, employeesByNormalizedName: Map<string, Employee[]>) {
  if (!value) return []

  return employeesByNormalizedName.get(normalizePersonName(value)) ?? []
}

function normalizePersonName(value: string) {
  return normalizeSearch(value)
    .replace(/[^a-z0-9 ]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
}

function DapicEmployeeLinkForm({
  dapicEmployee,
  employees,
  onSubmit,
  onCancel,
}: {
  dapicEmployee: DapicEmployee
  employees: Employee[]
  onSubmit: (employeeId: string) => void
  onCancel: () => void
}) {
  return (
    <form
      className="entry-form"
      onSubmit={(event) => {
        event.preventDefault()
        const data = new FormData(event.currentTarget)
        onSubmit(String(data.get('employeeId')))
      }}
    >
      <label className="span-2">
        Funcionario Dapic
        <input value={dapicEmployee.name} disabled />
      </label>
      <label className="span-2">
        Colaborador RH Folha
        <select name="employeeId" defaultValue={dapicEmployee.employeeId ?? selectDefaultValue(employees)} required>
          {employees.length !== 1 && <option value="">Selecione o colaborador</option>}
          {employees.map((employee) => (
            <option key={employee.id} value={employee.id}>
              {employee.registration} - {employee.name}
            </option>
          ))}
        </select>
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit" disabled={employees.length === 0}>
          Salvar vinculo
        </button>
      </div>
    </form>
  )
}

function FeedbackBanner({ message }: { message: FeedbackMessage }) {
  return <div className={`feedback feedback-${message.type}`}>{message.text}</div>
}

function LoginView({
  message,
  onSubmit,
}: {
  message: string
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}) {
  return (
    <main className="login-shell">
      <section className="login-panel">
        <div>
          <p className="brand-kicker">RH Folha</p>
          <h1>Acesso ao sistema</h1>
          <span>Use seu usuario autorizado para acessar dados de folha e colaboradores.</span>
        </div>
        {message && <div className="alert">{message}</div>}
        <form className="login-form" onSubmit={onSubmit}>
          <label>
            E-mail
            <input name="email" type="email" defaultValue="admin@rhfolha.local" autoComplete="username" required />
          </label>
          <label>
            Senha
            <input name="password" type="password" autoComplete="current-password" required />
          </label>
          <button type="submit">Entrar</button>
        </form>
      </section>
    </main>
  )
}

function UserCreateForm({
  companies,
  onSubmit,
  onCancel,
}: {
  companies: Company[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void | Promise<void>
  onCancel: () => void
}) {
  return (
    <form className="entry-form" onSubmit={(event) => void onSubmit(event)}>
      <label>
        Nome
        <input name="fullName" placeholder="Nome completo" required />
      </label>
      <label>
        E-mail
        <input name="email" type="email" placeholder="usuario@empresa.com" required />
      </label>
      <label>
        Senha inicial
        <input name="password" type="password" minLength={6} required />
      </label>
      <label>
        Perfil
        <RoleSelect name="role" defaultValue="rh_operacional" />
      </label>
      <label>
        Empresa
        <CompanySelect companies={companies} />
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit">Salvar usuario</button>
      </div>
    </form>
  )
}

function UserEditForm({
  user,
  companies,
  onSubmit,
  onCancel,
}: {
  user: SystemUser
  companies: Company[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void | Promise<void>
  onCancel: () => void
}) {
  return (
    <form className="entry-form" onSubmit={(event) => void onSubmit(event)}>
      <label>
        Nome
        <input name="fullName" defaultValue={user.fullName} required />
      </label>
      <label>
        E-mail
        <input value={user.email} disabled />
      </label>
      <label>
        Perfil
        <RoleSelect name="role" defaultValue={user.role} />
      </label>
      <label>
        Empresa
        <CompanySelect companies={companies} defaultValue={user.companyId ?? ''} />
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit">Salvar alteracao</button>
      </div>
    </form>
  )
}

function UserResetPasswordForm({
  user,
  onSubmit,
  onCancel,
}: {
  user: SystemUser
  onSubmit: (event: FormEvent<HTMLFormElement>) => void | Promise<void>
  onCancel: () => void
}) {
  return (
    <form className="entry-form" onSubmit={(event) => void onSubmit(event)}>
      <label className="span-2">
        Usuario
        <input value={`${user.fullName} - ${user.email}`} disabled />
      </label>
      <label>
        Nova senha
        <input name="password" type="password" minLength={6} required />
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit">Redefinir senha</button>
      </div>
    </form>
  )
}

function RoleSelect({ name, defaultValue }: { name: string; defaultValue: string }) {
  return (
    <select name={name} defaultValue={defaultValue} required>
      <option value="administrador">Administrador</option>
      <option value="rh_operacional">RH operacional</option>
      <option value="conferente">Conferente</option>
      <option value="leitura">Somente leitura</option>
    </select>
  )
}

function CompanySelect({ companies, defaultValue = '' }: { companies: Company[]; defaultValue?: string }) {
  return (
    <select name="companyId" defaultValue={defaultValue}>
      <option value="">Todas</option>
      {companies.map((company) => (
        <option key={company.id} value={company.id}>
          {company.tradeName ?? company.legalName}
        </option>
      ))}
    </select>
  )
}

function Metric({ label, value }: { label: string; value: string | number }) {
  return (
    <article className="metric">
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  )
}

function Panel({ title, children, className = '' }: { title: string; children: ReactNode; className?: string }) {
  return (
    <section className={`panel ${className}`.trim()}>
      <h3>{title}</h3>
      {children}
    </section>
  )
}

function Modal({
  title,
  children,
  onClose,
  size = 'default',
}: {
  title: string
  children: ReactNode
  onClose: () => void
  size?: 'default' | 'wide'
}) {
  return (
    <div className="modal-backdrop" role="presentation">
      <section className={`modal ${size === 'wide' ? 'modal-wide' : ''}`} role="dialog" aria-modal="true">
        <header className="modal-header">
          <h3>{title}</h3>
          <button type="button" className="table-action" onClick={onClose}>
            Fechar
          </button>
        </header>
        <div className="modal-body">{children}</div>
      </section>
    </div>
  )
}

function MessageModal({ message, onClose }: { message: string; onClose: () => void }) {
  return (
    <div className="modal-backdrop message-modal-backdrop" role="presentation">
      <section className="message-modal" role="alertdialog" aria-modal="true">
        <h3>Confirmacao</h3>
        <p>{message}</p>
        <button type="button" onClick={onClose}>
          OK
        </button>
      </section>
    </div>
  )
}

function DataTable({
  title,
  action,
  columns,
  rows,
  pageSize,
}: {
  title: string
  action?: ReactNode
  columns: string[]
  rows: Array<Array<string | ReactNode>>
  pageSize?: number
}) {
  const [page, setPage] = useState(1)
  const isPaginated = Boolean(pageSize && rows.length > pageSize)
  const effectivePageSize = pageSize ?? rows.length
  const totalPages = Math.max(Math.ceil(rows.length / effectivePageSize), 1)
  const safePage = Math.min(page, totalPages)
  const visibleRows = isPaginated ? rows.slice((safePage - 1) * effectivePageSize, safePage * effectivePageSize) : rows

  useEffect(() => {
    setPage(1)
  }, [rows.length, pageSize])

  return (
    <section className="panel table-panel">
      <header className="table-panel-header">
        <h3>{title}</h3>
        {action}
      </header>
      <div className="table-scroll">
        <table>
          <thead>
            <tr>
              {columns.map((column) => (
                <th key={column}>{column}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {visibleRows.map((row, index) => (
              <tr key={`${title}-${index}`}>
                {row.map((cell, cellIndex) => (
                  <td key={`${title}-${index}-${cellIndex}`}>{cell}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {isPaginated && (
        <PaginationControls
          page={safePage}
          totalPages={totalPages}
          pageSize={effectivePageSize}
          totalRecords={rows.length}
          onFirst={() => setPage(1)}
          onPrevious={() => setPage((currentPage) => Math.max(currentPage - 1, 1))}
          onNext={() => setPage((currentPage) => Math.min(currentPage + 1, totalPages))}
          onLast={() => setPage(totalPages)}
        />
      )}
    </section>
  )
}

function SearchableSelect({
  name,
  label,
  options,
  defaultValue = '',
  placeholder,
  allowEmpty = false,
}: {
  name: string
  label: string
  options: SelectOption[]
  defaultValue?: string
  placeholder: string
  allowEmpty?: boolean
}) {
  const [selectedValue, setSelectedValue] = useState(defaultValue)
  const [search, setSearch] = useState('')
  const selectedOption = options.find((option) => option.value === selectedValue)
  const normalizedSearch = normalizeSearch(search)
  const visibleOptions = options
    .filter(
      (option) =>
        normalizedSearch.length === 0 ||
        normalizeSearch(`${option.label} ${option.description ?? ''}`).includes(normalizedSearch),
    )
    .slice(0, 30)

  function selectOption(option: SelectOption | null) {
    setSelectedValue(option?.value ?? '')
    setSearch(option?.label ?? '')
  }

  return (
    <label className="searchable-select-field">
      {label}
      <input type="hidden" name={name} value={selectedValue} />
      <div className="searchable-select">
        <input
          value={search}
          onChange={(event) => {
            setSearch(event.target.value)
            if (selectedValue) {
              setSelectedValue('')
            }
          }}
          placeholder={selectedOption?.label ?? placeholder}
        />
        <div className="searchable-select-list">
          {allowEmpty && (
            <button type="button" onClick={() => selectOption(null)}>
              <strong>{placeholder}</strong>
            </button>
          )}
          {visibleOptions.map((option) => (
            <button
              key={option.value}
              type="button"
              className={option.value === selectedValue ? 'selected' : ''}
              onClick={() => selectOption(option)}
            >
              <strong>{option.label}</strong>
              {option.description && <span>{option.description}</span>}
            </button>
          ))}
          {visibleOptions.length === 0 && <span className="searchable-select-empty">Nenhum registro encontrado</span>}
        </div>
      </div>
    </label>
  )
}

function PaginationControls({
  page,
  totalPages,
  pageSize,
  totalRecords,
  onFirst,
  onPrevious,
  onNext,
  onLast,
}: {
  page: number
  totalPages: number
  pageSize: number
  totalRecords: number
  onFirst: () => void
  onPrevious: () => void
  onNext: () => void
  onLast: () => void
}) {
  return (
    <div className="pagination-bar">
      <span>
        Pagina {page} de {totalPages} | {pageSize} por pagina | {totalRecords} registro(s)
      </span>
      <div className="pagination-actions">
        <button type="button" className="secondary-button" disabled={page === 1} onClick={onFirst}>
          Primeira
        </button>
        <button type="button" className="secondary-button" disabled={page === 1} onClick={onPrevious}>
          Anterior
        </button>
        <button type="button" className="secondary-button" disabled={page === totalPages} onClick={onNext}>
          Proxima
        </button>
        <button type="button" className="secondary-button" disabled={page === totalPages} onClick={onLast}>
          Ultima
        </button>
      </div>
    </div>
  )
}

function DapicEmployeeCreateForm({
  dapicEmployee,
  departments,
  jobPositions,
  onSubmit,
  onCancel,
}: {
  dapicEmployee: DapicEmployee
  departments: Department[]
  jobPositions: JobPosition[]
  onSubmit: (payload: CreateLinkedEmployeePayload) => void
  onCancel: () => void
}) {
  const activeDepartments = departments.filter((department) => department.isActive)
  const activeJobPositions = jobPositions.filter((jobPosition) => jobPosition.isActive)

  return (
    <form
      className="employee-form"
      onSubmit={(event) => {
        event.preventDefault()
        const data = new FormData(event.currentTarget)
        const compensationModel = String(data.get('compensationModel'))
        onSubmit({
          departmentId: String(data.get('departmentId')),
          jobPositionId: String(data.get('jobPositionId')),
          registration: String(data.get('registration')),
          name: String(data.get('name')),
          documentNumber: String(data.get('documentNumber')),
          admissionDate: String(data.get('admissionDate')),
          compensationModel,
          baseSalary: compensationModel === 'mensalista' ? Number(data.get('baseSalary')) : 0,
          productionUnitValue: compensationModel === 'producao' ? Number(data.get('productionUnitValue')) : 0,
        })
      }}
    >
      <label>
        Matricula
        <input name="registration" placeholder="Ex: 0123" required />
      </label>
      <label className="span-2">
        Nome
        <input name="name" defaultValue={dapicEmployee.name} required />
      </label>
      <label>
        CPF/documento
        <input name="documentNumber" placeholder="Documento obrigatorio" required />
      </label>
      <label>
        Admissao
        <input name="admissionDate" type="date" defaultValue={todayInputDate()} required />
      </label>
      <label>
        Modelo
        <select name="compensationModel" defaultValue="producao" required>
          <option value="producao">Producao</option>
          <option value="mensalista">Mensalista</option>
        </select>
      </label>
      <label>
        Setor
        <select name="departmentId" defaultValue={selectDefaultValue(activeDepartments)} required>
          {activeDepartments.length !== 1 && <option value="">Selecione o setor</option>}
          {activeDepartments.map((department) => (
            <option key={department.id} value={department.id}>
              {department.internalCode} - {department.name}
            </option>
          ))}
        </select>
      </label>
      <label>
        Cargo
        <select name="jobPositionId" defaultValue={selectDefaultValue(activeJobPositions)} required>
          {activeJobPositions.length !== 1 && <option value="">Selecione o cargo</option>}
          {activeJobPositions.map((jobPosition) => (
            <option key={jobPosition.id} value={jobPosition.id}>
              {jobPosition.internalCode} - {jobPosition.name}
            </option>
          ))}
        </select>
      </label>
      <label>
        Salario mensal
        <input name="baseSalary" type="number" min="0" step="0.01" defaultValue="0" />
      </label>
      <label>
        Valor producao
        <input name="productionUnitValue" type="number" min="0" step="0.01" defaultValue="0" />
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit" disabled={activeDepartments.length === 0 || activeJobPositions.length === 0}>
          Criar e vincular
        </button>
      </div>
    </form>
  )
}

function FixedPayrollEntryEditForm({
  entry,
  fixedRubrics,
  onSubmit,
  onCancel,
}: {
  entry: FixedPayrollEntry
  fixedRubrics: Rubric[]
  onSubmit: (event: FormEvent<HTMLFormElement>) => void | Promise<void>
  onCancel: () => void
}) {
  const hasCurrentRubric = fixedRubrics.some((rubric) => rubric.id === entry.rubricId)

  return (
    <form className="entry-form" onSubmit={(event) => void onSubmit(event)}>
      <label className="span-2">
        Colaborador
        <input value={`${entry.employeeRegistration} - ${entry.employeeName}`} disabled />
      </label>
      <label>
        Rubrica
        <select name="rubricId" defaultValue={entry.rubricId} required>
          {!hasCurrentRubric && (
            <option value={entry.rubricId}>
              {entry.rubricCode} - {entry.rubricName}
            </option>
          )}
          {fixedRubrics.map((rubric) => (
            <option key={rubric.id} value={rubric.id}>
              {rubric.code} - {rubric.name}
            </option>
          ))}
        </select>
      </label>
      <label>
        Inicio vigencia
        <input name="startsOn" type="date" defaultValue={entry.startsOn} required />
      </label>
      <label>
        Fim vigencia
        <input name="endsOn" type="date" defaultValue={entry.endsOn ?? ''} />
      </label>
      <label>
        Valor
        <input name="amount" type="number" min="0.01" step="0.01" defaultValue={entry.amount} required />
      </label>
      <label>
        Quantidade
        <input name="quantity" type="number" min="0" step="0.0001" defaultValue={entry.quantity ?? ''} placeholder="Opcional" />
      </label>
      <label className="span-2">
        Observacao
        <input name="notes" defaultValue={entry.notes ?? ''} placeholder="Ex: desconto mensal recorrente" />
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit" disabled={fixedRubrics.length === 0 && !hasCurrentRubric}>
          Salvar alteracao
        </button>
      </div>
    </form>
  )
}

function FixedPayrollEntryCloseForm({
  entry,
  onSubmit,
  onCancel,
}: {
  entry: FixedPayrollEntry
  onSubmit: (event: FormEvent<HTMLFormElement>) => void | Promise<void>
  onCancel: () => void
}) {
  return (
    <form className="entry-form" onSubmit={(event) => void onSubmit(event)}>
      <label className="span-2">
        Lancamento fixo
        <input value={`${entry.rubricCode} - ${entry.rubricName} | ${entry.employeeRegistration} - ${entry.employeeName}`} disabled />
      </label>
      <label>
        Fim vigencia
        <input name="endsOn" type="date" defaultValue={entry.endsOn ?? new Date().toISOString().slice(0, 10)} required />
      </label>
      <div className="modal-actions">
        <button type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button type="submit">Encerrar vigencia</button>
      </div>
    </form>
  )
}

function summarizePayrollByEmployee(items: PayrollCalculationItem[]) {
  return Array.from(
    items.reduce((map, item) => {
      const current = map.get(item.employeeId) ?? {
        employeeId: item.employeeId,
        employee: `${item.employeeRegistration} - ${item.employeeName}`,
        registration: item.employeeRegistration,
        name: item.employeeName,
        proventos: 0,
        descontos: 0,
        liquido: 0,
        baseFgts: 0,
        fgts: 0,
        baseInss: 0,
        inss: 0,
        baseIrrf: 0,
        irrf: 0,
        items: 0,
      }

      if (item.rubricType === 'provento') {
        current.proventos += item.amount
      }

      if (item.rubricType === 'desconto') {
        current.descontos += item.amount
      }

      if (item.origin === 'sistema_fgts') {
        current.baseFgts = item.baseAmount ?? 0
        current.fgts = item.amount
      }

      if (item.origin === 'sistema_inss') {
        current.baseInss = item.baseAmount ?? 0
        current.inss = item.amount
      }

      if (item.origin === 'sistema_irrf') {
        current.baseIrrf = item.baseAmount ?? 0
        current.irrf = item.amount
      }

      current.liquido = current.proventos - current.descontos
      current.items += 1
      map.set(item.employeeId, current)

      return map
    }, new Map<string, EmployeePayrollSummary>()),
  )
    .map(([, summary]) => summary)
    .sort((left, right) => left.name.localeCompare(right.name, 'pt-BR'))
}

function summarizePayrollByRubric(items: PayrollCalculationItem[]) {
  return Array.from(
    items.reduce((map, item) => {
      const key = `${item.rubricCode}-${item.rubricName}-${item.rubricType}`
      const current = map.get(key) ?? {
        code: item.rubricCode,
        name: item.rubricName,
        type: item.rubricType,
        amount: 0,
        items: 0,
      }

      current.amount += item.amount
      current.items += 1
      map.set(key, current)

      return map
    }, new Map<string, { code: string; name: string; type: string; amount: number; items: number }>()),
  )
    .map(([, row]) => row)
    .sort((left, right) => left.code.localeCompare(right.code, 'pt-BR'))
}

function summarizePayrollByStructure(items: PayrollCalculationItem[], employees: Employee[]) {
  const employeesById = new Map(employees.map((employee) => [employee.id, employee]))

  return Array.from(
    summarizePayrollByEmployee(items).reduce((map, summary) => {
      const employee = employeesById.get(summary.employeeId)
      const department = employee?.departmentName ?? 'Sem setor'
      const jobPosition = employee?.jobPositionName ?? 'Sem cargo'
      const key = `${department}-${jobPosition}`
      const current = map.get(key) ?? {
        department,
        jobPosition,
        employees: 0,
        proventos: 0,
        descontos: 0,
        liquido: 0,
      }

      current.employees += 1
      current.proventos += summary.proventos
      current.descontos += summary.descontos
      current.liquido += summary.liquido
      map.set(key, current)

      return map
    }, new Map<string, { department: string; jobPosition: string; employees: number; proventos: number; descontos: number; liquido: number }>()),
  )
    .map(([, row]) => row)
    .sort((left, right) => `${left.department}-${left.jobPosition}`.localeCompare(`${right.department}-${right.jobPosition}`, 'pt-BR'))
}

function exportCsv(fileName: string, rows: Array<Array<string | number>>) {
  const csv = rows
    .map((row) =>
      row
        .map((cell) => {
          const value = typeof cell === 'number' ? String(cell).replace('.', ',') : cell
          return `"${value.replaceAll('"', '""')}"`
        })
        .join(';'),
    )
    .join('\r\n')

  const blob = new Blob([`\ufeff${csv}`], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')

  link.href = url
  link.download = fileName
  link.click()
  URL.revokeObjectURL(url)
}

function pageTitle(view: View) {
  const titles: Record<View, string> = {
    dashboard: 'Dashboard',
    employees: 'Colaboradores',
    structure: 'Estrutura',
    rubrics: 'Rubricas',
    periods: 'Competencias',
    entries: 'Lancamentos',
    conference: 'Conferencia',
    reports: 'Relatorios',
    settings: 'Configuracoes',
  }

  return titles[view]
}

function labelRubricType(value: string) {
  const labels: Record<string, string> = {
    provento: 'Provento',
    desconto: 'Desconto',
    informativa: 'Informativa',
    base: 'Base',
    patronal: 'Patronal',
  }

  return labels[value] ?? value
}

function formatIncidences(rubric: Rubric) {
  const validity = rubric.currentValidity
  if (!validity) return '-'

  const incidences = [
    validity.incidenceInss ? 'INSS' : null,
    validity.incidenceFgts ? 'FGTS' : null,
    validity.incidenceIrrf ? 'IRRF' : null,
    validity.incidenceDsr ? 'DSR' : null,
  ].filter(Boolean)

  return incidences.length > 0 ? incidences.join(', ') : 'Sem incidencia'
}

function formatRubricUsage(rubric: Rubric) {
  const usages = [
    rubric.allowsManualEntry ? 'Manual' : null,
    rubric.allowsMassEntry ? 'Massa' : null,
    rubric.allowsFixedEntry ? 'Fixo' : null,
  ].filter(Boolean)

  return usages.length > 0 ? usages.join(', ') : 'Sistema'
}

function labelCompensation(value: string) {
  return value === 'producao' ? 'Producao' : 'Mensalista'
}

function labelUserRole(value: string) {
  const labels: Record<string, string> = {
    administrador: 'Administrador',
    rh_operacional: 'RH operacional',
    conferente: 'Conferente',
    leitura: 'Somente leitura',
  }

  return labels[value] ?? value
}

function labelAuditAction(value: string) {
  const labels: Record<string, string> = {
    'user.create': 'Criou usuario',
    'user.update': 'Atualizou usuario',
    'user.reset_password': 'Redefiniu senha',
    'user.activate': 'Reativou usuario',
    'user.deactivate': 'Inativou usuario',
    'fixed_entry.create': 'Criou fixo',
    'fixed_entry.update': 'Atualizou fixo',
    'fixed_entry.close': 'Encerrou fixo',
    'fixed_entry.activate': 'Reativou fixo',
    'fixed_entry.deactivate': 'Inativou fixo',
    'payroll_period.create': 'Abriu competencia',
    'payroll_period.calculate': 'Calculou folha',
    'payroll_period.approve': 'Aprovou folha',
    'payroll_period.close': 'Fechou competencia',
    'payroll_period.reopen': 'Reabriu competencia',
    'employee.photo_update': 'Atualizou foto',
    'employee.photo_upload': 'Enviou foto',
    'employee.responsible_update': 'Atualizou responsavel',
    'employee_event.create': 'Registrou evento',
    'employee_event.cancel': 'Cancelou evento',
    'statutory_table.create': 'Criou tabela legal',
    'statutory_table.duplicate': 'Criou nova vigencia',
    'statutory_table.activate': 'Reativou tabela legal',
    'statutory_table.deactivate': 'Inativou tabela legal',
    'production_rate_table.create': 'Criou tabela de producao',
    'production_rate_table.update': 'Atualizou tabela de producao',
    'production_rate_table.duplicate': 'Criou vigencia de producao',
    'production_rate_table.activate': 'Ativou tabela de producao',
    'production_rate_table.deactivate': 'Inativou tabela de producao',
    'production_entry.create': 'Criou apontamento',
    'production_entry.approve': 'Aprovou apontamento',
    'production_entry.cancel': 'Cancelou apontamento',
    'dapic.employee_link': 'Vinculou funcionario Dapic',
    'dapic.employee_ignore': 'Ignorou funcionario Dapic',
    'dapic.employee_reset_link': 'Redefiniu funcionario Dapic',
  }

  return labels[value] ?? value
}

function inferFeedbackType(text: string): FeedbackType {
  const normalized = normalizeSearch(text)

  if (
    normalized.includes('nao foi possivel') ||
    normalized.includes('erro') ||
    normalized.includes('falha') ||
    normalized.includes('invalid')
  ) {
    return 'error'
  }

  if (
    normalized.includes('informe') ||
    normalized.includes('nao pode') ||
    normalized.includes('precisa') ||
    normalized.includes('reabra') ||
    normalized.includes('atencao')
  ) {
    return 'warning'
  }

  return 'success'
}

function isAdmin(role: string) {
  return role === 'administrador'
}

function canOperateHr(role: string) {
  return role === 'administrador' || role === 'rh_operacional'
}

function canWorkPayroll(role: string) {
  return role === 'administrador' || role === 'rh_operacional' || role === 'conferente'
}

function canApprovePayroll(role: string) {
  return role === 'administrador' || role === 'conferente'
}

function normalizeSearch(value: string) {
  return value
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim()
}

function selectDefaultValue(options: Array<{ id: string }>) {
  return options.length === 1 ? options[0].id : ''
}

function assetUrl(value: string) {
  if (value.startsWith('http://') || value.startsWith('https://')) {
    return value
  }

  if (value.startsWith('/uploads') && window.location.port === '5173') {
    return `http://localhost:5086${value}`
  }

  return value
}

function labelEmployeeEventType(value: string) {
  const labels: Record<string, string> = {
    curso: 'Curso',
    treinamento: 'Treinamento',
    advertencia: 'Advertencia',
    indisciplina: 'Indisciplina',
    elogio: 'Elogio',
    observacao: 'Observacao',
    mudanca_operacional: 'Mudanca operacional',
    outro: 'Outro',
  }

  return labels[value] ?? value
}

function initials(name: string) {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
}

function labelStatutoryTableType(value: string) {
  const labels: Record<string, string> = {
    inss: 'INSS',
    irrf: 'IRRF',
    irrf_reducao: 'IRRF reducao',
    fgts: 'FGTS',
    fgts_saque_aniversario: 'FGTS saque-aniversario',
  }

  return labels[value] ?? value.toUpperCase()
}

function labelDapicResource(value: string) {
  const labels: Record<string, string> = {
    employees: 'Funcionarios',
    funcionarios: 'Funcionarios',
    products: 'Produtos',
    produtos: 'Produtos',
    operations: 'Operacoes',
    operacoes: 'Operacoes',
    cells: 'Celulas',
    celulas: 'Celulas',
    orders: 'Ordens de producao',
    ordens: 'Ordens de producao',
    ordensproducao: 'Ordens de producao',
    productionorders: 'Ordens de producao',
  }

  return labels[value.toLowerCase()] ?? value
}

function labelDapicStatus(value: string) {
  const labels: Record<string, string> = {
    active: 'Ativa',
    inactive: 'Inativa',
    error: 'Erro',
    running: 'Em andamento',
    success: 'Sucesso',
    completed: 'Concluida',
    failed: 'Falha',
    unknown: 'Nao informado',
    waitingstart: 'Aguardando inicio',
    inproduction: 'Em producao',
    finished: 'Finalizada',
    canceled: 'Cancelada',
  }

  return labels[value.toLowerCase()] ?? value
}

function labelDapicLinkStatus(value: string) {
  const labels: Record<string, string> = {
    pendente: 'Pendente',
    vinculado: 'Vinculado',
    ignorado: 'Ignorado',
  }

  return labels[value] ?? value
}

function formatStatutoryRanges(ranges: StatutoryTableRange[]) {
  if (ranges.length === 0) return '-'

  return ranges
    .map((range) => {
      const upperLimit = range.upperLimit === null ? 'sem teto' : formatCurrency(range.upperLimit)
      return `${formatCurrency(range.lowerLimit)} a ${upperLimit}: ${formatNumber(range.ratePercent)}%`
    })
    .join(' | ')
}

function formatProductionRateRules(rates: ProductionRate[]) {
  if (rates.length === 0) return '-'

  return rates
    .slice(0, 3)
    .map((rate) => {
      const criteria = [
        rate.productReference,
        rate.operationName,
        rate.cellName,
        rate.departmentName,
        rate.jobPositionName,
      ].filter(Boolean)

      return `${criteria.length > 0 ? criteria.join(' / ') : 'Regra geral'}: ${formatCurrency(rate.unitValue)}`
    })
    .concat(rates.length > 3 ? [`+${rates.length - 3} regra(s)`] : [])
    .join(' | ')
}

function buildProductionRateTablePayload(form: HTMLFormElement, companyId: string) {
  const data = new FormData(form)
  const rowCount = form.querySelectorAll('input[name^="productionUnitValue"]').length
  const rates = Array.from({ length: rowCount }, (_, index) => index + 1)
    .map((index) => {
      const unitValue = Number(data.get(`productionUnitValue${index}`) || 0)
      const minimumQuantityValue = String(data.get(`productionMinimumQuantity${index}`) ?? '')
      const maximumQuantityValue = String(data.get(`productionMaximumQuantity${index}`) ?? '')

      return {
        productionProductId: data.get(`productionProductId${index}`) || null,
        productionOperationId: data.get(`productionOperationId${index}`) || null,
        productionCellId: data.get(`productionCellId${index}`) || null,
        departmentId: data.get(`productionDepartmentId${index}`) || null,
        jobPositionId: data.get(`productionJobPositionId${index}`) || null,
        unitValue,
        minimumQuantity: minimumQuantityValue ? Number(minimumQuantityValue) : null,
        maximumQuantity: maximumQuantityValue ? Number(maximumQuantityValue) : null,
        notes: data.get(`productionRateNotes${index}`) || null,
      }
    })
    .filter((rate) => rate.unitValue > 0)

  return {
    companyId,
    name: data.get('name'),
    effectiveFrom: data.get('effectiveFrom'),
    effectiveTo: data.get('effectiveTo') || null,
    notes: data.get('notes') || null,
    rates,
  }
}

function formatProductionQuantityRange(rate: ProductionRate) {
  if (rate.minimumQuantity === null && rate.maximumQuantity === null) return 'Qualquer quantidade'

  if (rate.minimumQuantity !== null && rate.maximumQuantity !== null) {
    return `${formatNumber(rate.minimumQuantity)} a ${formatNumber(rate.maximumQuantity)}`
  }

  if (rate.minimumQuantity !== null) return `A partir de ${formatNumber(rate.minimumQuantity)}`

  return `Ate ${formatNumber(rate.maximumQuantity ?? 0)}`
}

function formatCurrency(value: number) {
  return value.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  })
}

function roundCurrency(value: number) {
  return Math.round(value * 100) / 100
}

function formatNumber(value: number) {
  return value.toLocaleString('pt-BR', {
    maximumFractionDigits: 4,
  })
}

function labelPeriodStatus(value: string) {
  const labels: Record<string, string> = {
    aberta: 'Aberta',
    em_calculo: 'Em calculo',
    calculada: 'Calculada',
    aprovada: 'Aprovada',
    fechada: 'Fechada',
    reaberta: 'Reaberta',
    cancelada: 'Cancelada',
  }

  return labels[value] ?? value
}

function isPeriodOpen(status: string) {
  return status === 'aberta' || status === 'reaberta'
}

function statusBadge(value: string) {
  return <span className={`status-badge status-${value}`}>{labelPeriodStatus(value)}</span>
}

function activeBadge(isActive: boolean) {
  return <span className={`status-badge ${isActive ? 'status-active' : 'status-inactive'}`}>{isActive ? 'Ativo' : 'Inativo'}</span>
}

function productionRateStatusBadge(value: string) {
  const label = labelProductionRateStatus(value)
  const statusClass = value === 'Active' ? 'status-active' : value === 'Inactive' ? 'status-inactive' : 'status-warning'
  return <span className={`status-badge ${statusClass}`}>{label}</span>
}

function productionEntryStatusBadge(value: string) {
  const labels: Record<string, string> = {
    Draft: 'Rascunho',
    PendingReview: 'Pendente',
    Approved: 'Aprovado',
    Rejected: 'Rejeitado',
    IntegratedIntoPayroll: 'Integrado',
    Canceled: 'Cancelado',
  }
  const statusClass =
    value === 'Approved' || value === 'IntegratedIntoPayroll'
      ? 'status-active'
      : value === 'Canceled' || value === 'Rejected'
        ? 'status-inactive'
        : 'status-warning'

  return <span className={`status-badge ${statusClass}`}>{labels[value] ?? value}</span>
}

function labelProductionRateStatus(value: string) {
  const labels: Record<string, string> = {
    Draft: 'Rascunho',
    Active: 'Ativa',
    Inactive: 'Inativa',
  }

  return labels[value] ?? value
}

function originBadge(origin: 'manual' | 'dapic') {
  return <span className={`origin-badge origin-${origin}`}>{origin === 'dapic' ? 'Dapic' : 'Manual'}</span>
}

function dapicStatusBadge(value: string) {
  const normalized = normalizeSearch(value).replaceAll(' ', '-')
  return <span className={`status-badge dapic-status dapic-status-${normalized}`}>{labelDapicStatus(value)}</span>
}

function dapicLinkStatusBadge(value: string) {
  return <span className={`status-badge dapic-link-status dapic-link-${value}`}>{labelDapicLinkStatus(value)}</span>
}

function entryStatusBadge(value: string) {
  return <span className={`status-badge status-${value}`}>{labelEntryStatus(value)}</span>
}

function eventStatusBadge(value: string) {
  return <span className={`status-badge status-${value}`}>{labelEmployeeEventStatus(value)}</span>
}

function labelEmployeeEventStatus(value: string) {
  const labels: Record<string, string> = {
    ativo: 'Ativo',
    cancelado: 'Cancelado',
  }

  return labels[value] ?? value
}

function labelEntryOrigin(value: string) {
  const labels: Record<string, string> = {
    manual: 'Manual',
    massa: 'Massa',
  }

  return labels[value] ?? value
}

function labelCalculationOrigin(value: string) {
  const labels: Record<string, string> = {
    salario: 'Salario',
    manual: 'Manual',
    massa: 'Massa',
    fixo: 'Fixo',
    sistema_fgts: 'FGTS',
    sistema_inss: 'INSS',
    sistema_irrf: 'IRRF',
  }

  return labels[value] ?? value
}

function labelEntryStatus(value: string) {
  const labels: Record<string, string> = {
    rascunho: 'Rascunho',
    aprovado: 'Aprovado',
    cancelado: 'Cancelado',
  }

  return labels[value] ?? value
}

function actionButton(label: string, onClick: () => void, disabled = false) {
  return (
    <button type="button" className="table-action" onClick={onClick} disabled={disabled}>
      {label}
    </button>
  )
}

function fixedPayrollEntryActions(
  entry: FixedPayrollEntry,
  onEdit: (entry: FixedPayrollEntry) => void,
  onClose: (entry: FixedPayrollEntry) => void,
  onToggle: (entry: FixedPayrollEntry) => void,
) {
  return (
    <div className="table-actions">
      {actionButton('Editar', () => onEdit(entry))}
      {actionButton('Encerrar', () => onClose(entry), Boolean(entry.endsOn))}
      {actionButton(entry.isActive ? 'Inativar' : 'Reativar', () => onToggle(entry))}
    </div>
  )
}

function periodActions(
  period: PayrollPeriod,
  hasCalculation: boolean,
  onCalculate: (period: PayrollPeriod) => void,
  onConference: (period: PayrollPeriod) => void,
  onApprove: (period: PayrollPeriod) => void,
  onClose: (period: PayrollPeriod) => void,
  onReopen: (period: PayrollPeriod) => void,
  canCalculate: boolean,
  canApprove: boolean,
  canCloseOrReopen: boolean,
) {
  if (period.status === 'fechada') {
    return (
      <div className="table-actions">
        {actionButton('Conferir', () => onConference(period), !hasCalculation)}
        {canCloseOrReopen && actionButton('Reabrir', () => onReopen(period))}
      </div>
    )
  }

  return (
    <div className="table-actions">
      {canCalculate &&
        actionButton('Calcular', () => onCalculate(period), !isPeriodOpen(period.status) && period.status !== 'calculada')}
      {actionButton('Conferir', () => onConference(period), !hasCalculation)}
      {canApprove && actionButton('Aprovar', () => onApprove(period), period.status !== 'calculada' || !hasCalculation)}
      {canCloseOrReopen && actionButton('Fechar', () => onClose(period), period.status !== 'aprovada')}
    </div>
  )
}

function formatDate(value: string) {
  return new Date(`${value}T00:00:00`).toLocaleDateString('pt-BR')
}

function nextYearStart(value: string) {
  const year = Number(value.slice(0, 4))
  return `${year + 1}-01-01`
}

function nextMonthStart(value: string) {
  const date = new Date(`${value}T00:00:00`)
  return toInputDate(new Date(date.getFullYear(), date.getMonth() + 1, 1))
}

function currentMonthStart() {
  const now = new Date()
  return toInputDate(new Date(now.getFullYear(), now.getMonth(), 1))
}

function todayInputDate() {
  return toInputDate(new Date())
}

function toInputDate(value: Date) {
  const year = value.getFullYear()
  const month = String(value.getMonth() + 1).padStart(2, '0')
  const day = String(value.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  })
}

export default App
