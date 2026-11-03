# واجهة منصة الاتصالات

هذه الواجهة مبنية باستخدام `React` و`TypeScript` و`TanStack Start`.

## مسؤولية الواجهة

الواجهة مسؤولة عن:

- عرض بيانات الأبراج والحوادث والطاقة والكربون.
- إرسال طلبات إلى الـBackend API.
- عرض حالات التحميل والأخطاء والبيانات الفارغة.
- البحث والتصفية وترتيب البيانات داخل الجداول.
- عرض الرسومات والبطاقات والتقارير.

لا تحتوي الواجهة الرئيسية على Mock Data. مصدر البيانات الوحيد هو الـBackend.

## التشغيل محليًا

من داخل هذا المجلد شغّل:

```powershell
npm install
npm run dev
```

Run the demo version:

```powershell
npm run dev:demo
```

Run the production version against the real backend API:

```powershell
npm run dev:production
```

The environment files are `.env.demo` and `.env.production`.

تستخدم الواجهة عنوان الـAPI التالي افتراضيًا:

```text
http://localhost:5000/api
```

يمكن تغيير العنوان من خلال متغير البيئة:

```text
VITE_API_BASE_URL
```

يجب تشغيل الـBackend قبل فتح الواجهة حتى تظهر البيانات الحقيقية من قاعدة البيانات.

## فحوصات الواجهة

```powershell
npm run lint
npx tsc --noEmit
```

## الصفحات الموجودة

- النظرة العامة.
- الحوادث والتنبيهات.
- الطاقة والتكلفة.
- تقرير الكربون.
- تفاصيل البرج وتحليله.

الواجهة لا تنفذ حسابات الأعمال الأساسية ولا تحدد حالة البرج. هذه المسؤوليات موجودة في الـBackend.
