function MainSettings(props) {
  return (
    <Page>
      <Section
        title={<Text bold align="center">Connection Settings</Text>}>
        <TextInput
          settingsKey="servercode"
          label="Your Local IP"
        />
      </Section>
    </Page>
  );
}
registerSettingsPage(MainSettings);