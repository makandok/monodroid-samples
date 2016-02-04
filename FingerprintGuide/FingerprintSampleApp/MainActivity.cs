using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using BasicFingerPrintSample.FingerprintAuthSample;
using BasicFingerPrintSample.FingerprintManagerAPISample;

namespace BasicFingerPrintSample
{
    public delegate void OnTitleItemClicked(object sender, EventArgs e);

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        // ReSharper disable once InconsistentNaming
        static readonly string TAG = "X:" + typeof (MainActivity).Name;
        RecyclerView.Adapter _adapter;
        RecyclerView.LayoutManager _layoutManager;
        RecyclerView _recyclerView;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            _layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(_layoutManager);

            _adapter = new TitleAdapter(this);
            _recyclerView.SetAdapter(_adapter);
        }

        class TitleAdapter : RecyclerView.Adapter
        {
            /// <summary>
            ///     A title item is a tuple that consists of the title (a String),
            ///     a description (a String), and the Acivity to launch (a Type).
            /// </summary>
            static readonly Tuple<string, string, Type>[] TitleItems = new Tuple<string, string, Type>[2];

            readonly Activity _activity;

            public TitleAdapter(Activity activity)
            {
                _activity = activity;

                // The rows that should show up in the RecyclerView.
                TitleItems[0] = BuildDatasetRow(Resource.String.activity_fingerprintmanagerapiactivity_title,
                                                Resource.String.activity_fingerprintmanagerapiactivity_description,
                                                typeof (FingerprintManagerApiActivity));
                TitleItems[1] = BuildDatasetRow(Resource.String.activity_fingeprintauthenticatoractivity_title,
                                                Resource.String.activity_fingeprintauthenticatoractivity_description,
                                                typeof (FingerprintAuthenticatorActivity));
            }

            public override int ItemCount
            {
                // ReSharper disable once ConvertPropertyToExpressionBody
                get { return TitleItems.Length; }
            }

            Tuple<string, string, Type> BuildDatasetRow(int titleResId, int descriptionResId, Type t)
            {
                string title = _activity.Resources.GetString(titleResId);
                string desc = _activity.Resources.GetString(descriptionResId);
                return new Tuple<string, string, Type>(title, desc, t);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.recyclerview_row, parent, false);

                v.Click += TitleItemClicked;
                TitleViewHolder vh = new TitleViewHolder(v);
                return vh;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                Tuple<string, string, Type> item = TitleItems[position];
                ((TitleViewHolder) holder).Display(item);
            }

            void TitleItemClicked(object sender, EventArgs e)
            {
                TitleViewHolder tvh = ((View) sender).Tag as TitleViewHolder;
                Intent i = new Intent(_activity, tvh.TypeOfActivity);
                Log.Verbose(TAG, "Display the activity {0}.", tvh.TypeOfActivity.Name);
                _activity.StartActivity(i);
            }
        }

        class TitleViewHolder : RecyclerView.ViewHolder
        {
            public TitleViewHolder(View v) : base(v)
            {
                Title = v.FindViewById<TextView>(Resource.Id.titleTextView);
                Description = v.FindViewById<TextView>(Resource.Id.descriptionTextView);
                v.Tag = this;
            }

            TextView Title { get; }
            TextView Description { get; }
            public Type TypeOfActivity { get; private set; }

            public void Display(Tuple<string, string, Type> item)
            {
                Title.Text = item.Item1;
                Description.Text = item.Item2;
                TypeOfActivity = item.Item3;
            }
        }
    }
}